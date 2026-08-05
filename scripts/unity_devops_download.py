import sys
import os
import time
import socket
import json
import urllib.request
import urllib.parse
import urllib.error
import base64
import zipfile

print("SCRIPT VERSION: 2026-08-04 BUILD 3", flush=True)
sys.stdout.flush()

api_key = os.environ.get("UNITY_DEVOPS_API_KEY", "").strip()
org_id = os.environ.get("UNITY_ORG_ID", "").strip()
project_id = os.environ.get("UNITY_PROJECT_ID", "").strip()
target_id = os.environ.get("UNITY_BUILD_TARGET_ID", "").strip() or "ios"
commit_sha = os.environ.get("GITHUB_SHA", "").strip()

if not api_key or not org_id or not project_id:
    print("ERROR: Missing required secrets! Please configure UNITY_DEVOPS_API_KEY, UNITY_ORG_ID, and UNITY_PROJECT_ID in GitHub Repository Secrets.")
    sys.exit(1)

base_url = f"https://build-api.cloud.unity3d.com/api/v1/orgs/{org_id}/projects/{project_id}/buildtargets/{target_id}"

basic_b64 = base64.b64encode((api_key + ":").encode("utf-8")).decode("utf-8")

auth_headers = [
    f"Basic {api_key}",
    f"Basic {basic_b64}",
    f"Bearer {api_key}"
]

working_auth_hdr = None

# Redirect handler that strips Auth header for external cloud storage S3/GCS redirects or host changes
class StripAuthRedirectHandler(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        new_req = super().redirect_request(req, fp, code, msg, headers, newurl)
        if new_req:
            parsed_new = urllib.parse.urlparse(newurl)
            parsed_req = urllib.parse.urlparse(req.full_url)
            if parsed_new.netloc != parsed_req.netloc or any(domain in newurl.lower() for domain in ["s3", "amazonaws", "googleapis", "cloudfront", "storage"]):
                if new_req.has_header("Authorization"):
                    new_req.remove_header("Authorization")
                if new_req.has_header("authorization"):
                    new_req.remove_header("authorization")
        return new_req

opener = urllib.request.build_opener(StripAuthRedirectHandler())

def make_request(url, method="GET", data=None):
    global working_auth_hdr
    print(f"Calling {method} {url}", flush=True)
    last_err = None
    headers_to_try = ([working_auth_hdr] if working_auth_hdr else []) + [h for h in auth_headers if h != working_auth_hdr]

    for auth_hdr in headers_to_try:
        req = urllib.request.Request(url, method=method)
        req.add_header("Authorization", auth_hdr)
        req.add_header("Accept", "application/json")
        if data:
            req.add_header("Content-Type", "application/json")
            req.data = json.dumps(data).encode("utf-8")

        auth_display = f"{auth_hdr[:15]}..." if len(auth_hdr) > 15 else auth_hdr
        accept_display = req.get_header("Accept") or "application/json"
        print(f"DEBUG Request: {method} {url} | Authorization: {auth_display} | Accept: {accept_display}", flush=True)

        try:
            print(f"[HTTP] {method} {url}", flush=True)
            with opener.open(req, timeout=30) as resp:
                status_code = resp.status
                resp_headers = resp.headers
                body_bytes = resp.read()
                body_text = body_bytes.decode("utf-8", errors="ignore")

                print(f"[HTTP RESPONSE STATUS] {status_code}", flush=True)
                print(f"[HTTP RESPONSE HEADERS]\n{resp_headers}", flush=True)
                print(f"[HTTP RESPONSE BODY]\n{body_text}", flush=True)

                working_auth_hdr = auth_hdr
                return json.loads(body_text), status_code
        except (socket.timeout, urllib.error.URLError) as e:
            if isinstance(e, socket.timeout) or (hasattr(e, "reason") and isinstance(e.reason, socket.timeout)):
                print(f"HTTP TIMEOUT for {url}", flush=True)
                sys.exit(1)
            if isinstance(e, urllib.error.HTTPError):
                pass
            else:
                print(f"HTTP Error/URLError: {e}", flush=True)
                raise RuntimeError(f"URL request failed for {url}: {e}") from e
        except urllib.error.HTTPError as e:
            last_err = e
            status_code = e.code
            err_headers = e.headers if hasattr(e, "headers") else {}
            body_text = e.read().decode("utf-8", errors="ignore")

            print(f"[HTTP ERROR STATUS] {status_code}", flush=True)
            print(f"[HTTP ERROR HEADERS]\n{err_headers}", flush=True)
            print(f"[HTTP ERROR BODY]\n{body_text}", flush=True)

            if e.code == 429:
                print(f"HTTP Error 429 Too Many Requests for URL {url}. Terminating immediately.", flush=True)
                sys.exit(1)
            if e.code == 401:
                continue
            raise RuntimeError(f"HTTP Error {e.code} for {url}:\n{body_text}") from e

    if last_err is not None:
        raise RuntimeError(f"HTTP Authentication Error: Could not authenticate with provided UNITY_DEVOPS_API_KEY. Last error: {last_err}") from last_err
    else:
        raise RuntimeError("HTTP Authentication Error: Could not authenticate with provided UNITY_DEVOPS_API_KEY.")

def parse_link_obj(obj):
    if not obj:
        return None, "GET"
    if isinstance(obj, str):
        return obj, "GET"
    if isinstance(obj, dict):
        href = obj.get("href") or obj.get("url") or obj.get("download_url")
        method = obj.get("method", "GET").upper()
        return href, method
    return None, "GET"

def find_in_artifacts_list(art_list, parse_link_obj_fn):
    candidate_url = None
    candidate_method = "GET"

    if isinstance(art_list, dict):
        if "artifacts" in art_list and isinstance(art_list["artifacts"], list):
            art_list = art_list["artifacts"]
        elif "files" in art_list:
            art_list = [art_list]
        else:
            art_list = [art_list]
    elif not isinstance(art_list, list):
        return None, "GET"

    for art in art_list:
        if not isinstance(art, dict):
            continue

        # Check if art itself is a file entry with href
        if "href" in art and ("filename" in art or "name" in art):
            fn = art.get("filename", "") or art.get("name", "")
            url, method = parse_link_obj_fn(art)
            if url:
                if fn.endswith(".ipa") or fn.endswith(".zip") or "build.ipa" in fn:
                    return url, method
                if not candidate_url:
                    candidate_url, candidate_method = url, method

        files = art.get("files", [])
        if isinstance(files, list):
            for file_info in files:
                if not isinstance(file_info, dict):
                    continue
                fn = file_info.get("filename", "") or file_info.get("name", "")
                url, method = parse_link_obj_fn(file_info)
                if not url and "download" in file_info:
                    url, method = parse_link_obj_fn(file_info["download"])
                if not url and "links" in file_info:
                    url, method = parse_link_obj_fn(file_info.get("links", {}).get("download"))

                if url:
                    if fn.endswith(".ipa") or fn.endswith(".zip") or "build.ipa" in fn:
                        return url, method
                    if not candidate_url:
                        candidate_url, candidate_method = url, method

        art_links = art.get("links", {}) if isinstance(art.get("links"), dict) else {}
        if "download" in art_links:
            url, method = parse_link_obj_fn(art_links["download"])
            if url and not candidate_url:
                candidate_url, candidate_method = url, method

        if "download" in art:
            url, method = parse_link_obj_fn(art["download"])
            if url and not candidate_url:
                candidate_url, candidate_method = url, method

    return candidate_url, candidate_method

def extract_download_info(b_info):
    ipa_url = None
    download_method = "GET"

    if not isinstance(b_info, dict):
        return None, download_method

    links = b_info.get("links", {}) if isinstance(b_info.get("links"), dict) else {}
    for link_key in ["download", "download_primary", "artifacts"]:
        if link_key in links:
            url, method = parse_link_obj(links[link_key])
            if url:
                ipa_url, download_method = url, method
                break

    if not ipa_url:
        b_artifacts = b_info.get("artifacts")
        if b_artifacts:
            ipa_url, download_method = find_in_artifacts_list(b_artifacts, parse_link_obj)

    if not ipa_url:
        b_files = b_info.get("files")
        if b_files:
            ipa_url, download_method = find_in_artifacts_list([{"files": b_files}], parse_link_obj)

    if ipa_url and ipa_url.startswith("/"):
        ipa_url = f"https://build-api.cloud.unity3d.com{ipa_url}"

    return ipa_url, download_method

def download_artifact(ipa_url, download_method, download_path):
    parsed = urllib.parse.urlparse(ipa_url)
    is_unity_api = "build-api.cloud.unity3d.com" in parsed.netloc or ipa_url.startswith("/")

    if is_unity_api:
        hdrs_to_try = ([working_auth_hdr] if working_auth_hdr else []) + [h for h in auth_headers if h != working_auth_hdr] + [None]
    else:
        hdrs_to_try = [None] + ([working_auth_hdr] if working_auth_hdr else []) + [h for h in auth_headers if h != working_auth_hdr]

    for auth_hdr in hdrs_to_try:
        req = urllib.request.Request(ipa_url, method=download_method)
        if auth_hdr:
            req.add_header("Authorization", auth_hdr)
        req.add_header("Accept", "*/*")

        auth_display = f"{auth_hdr[:15]}..." if auth_hdr and len(auth_hdr) > 15 else (auth_hdr or "None")
        accept_display = req.get_header("Accept") or "*/*"
        print(f"DEBUG Request: {download_method} {ipa_url} | Authorization: {auth_display} | Accept: {accept_display}", flush=True)

        try:
            print(f"[HTTP] {download_method} {ipa_url}", flush=True)
            with opener.open(req, timeout=30) as resp:
                status_code = resp.status
                resp_headers = dict(resp.headers)
                print(f"[HTTP OK] {status_code}", flush=True)
                print(f"[HTTP RESPONSE HEADERS]\n{resp.headers}", flush=True)
                with open(download_path, "wb") as out_file:
                    out_file.write(resp.read())
            return True, resp_headers
        except (socket.timeout, urllib.error.URLError) as e:
            if isinstance(e, socket.timeout) or (hasattr(e, "reason") and isinstance(e.reason, socket.timeout)):
                print(f"HTTP TIMEOUT for {ipa_url}", flush=True)
                sys.exit(1)
            print(f"Download attempt failed with URLError: {e}", flush=True)
            continue
        except urllib.error.HTTPError as e:
            if e.code == 429:
                print("HTTP Error 429 Too Many Requests during download. Terminating immediately.", flush=True)
                sys.exit(1)
            print(f"Download attempt with auth '{auth_hdr[:15] if auth_hdr else 'None'}' failed: HTTP {e.code}", flush=True)
            continue
        except Exception as e:
            print(f"Download attempt failed: {e}", flush=True)
            continue

    return False, {}

print(f"=== Checking existing Unity DevOps builds for target '{target_id}' ===")
build_num = None
existing_status = None
builds = []
active_statuses = ["created", "queued", "sentToBuilder", "building", "started"]

try:
    builds_resp, _ = make_request(f"{base_url}/builds?limit=10")
    if isinstance(builds_resp, list):
        builds = builds_resp
    elif isinstance(builds_resp, dict) and "builds" in builds_resp:
        builds = builds_resp["builds"]

    for b in builds:
        if not isinstance(b, dict):
            continue
        if b.get("lastBuiltRevision") == commit_sha or b.get("commit") == commit_sha:
            b_status = b.get("buildStatus")
            if b_status in active_statuses + ["success"]:
                build_num = b.get("build")
                existing_status = b_status
                print(f"Found existing build #{build_num} with status '{b_status}' for commit {commit_sha}", flush=True)
                break
except Exception as e:
    print(f"Notice querying builds: {e}", flush=True)

if not build_num:
    print(f"Triggering new Unity DevOps build for target '{target_id}' on commit {commit_sha}...", flush=True)
    trigger_data = {"clean": False, "commit": commit_sha, "label": f"GitHub Actions {commit_sha[:7]}"}
    try:
        resp, _ = make_request(f"{base_url}/builds", method="POST", data=trigger_data)

        pending_error_found = False
        extracted_build_num = None

        if isinstance(resp, list) and len(resp) > 0:
            first_item = resp[0]
            if isinstance(first_item, dict):
                err_msg = str(first_item.get("error", ""))
                if "already a build pending" in err_msg or "Cannot start build" in err_msg:
                    pending_error_found = True
                    print(f"Notice from POST /builds: {err_msg}", flush=True)
                else:
                    extracted_build_num = first_item.get("build")
        elif isinstance(resp, dict):
            err_msg = str(resp.get("error", ""))
            if "already a build pending" in err_msg or "Cannot start build" in err_msg:
                pending_error_found = True
                print(f"Notice from POST /builds: {err_msg}", flush=True)
            else:
                extracted_build_num = resp.get("build")

        if not pending_error_found and extracted_build_num is not None:
            build_num = extracted_build_num
            print(f"Build #{build_num} successfully queued on Unity DevOps!", flush=True)
        else:
            print("POST /builds returned pending error or invalid build number. Searching for active pending build in /builds...", flush=True)
            for b in builds:
                if isinstance(b, dict):
                    b_status = b.get("buildStatus")
                    if b_status in active_statuses and b.get("build") is not None:
                        build_num = b.get("build")
                        existing_status = b_status
                        print(f"Found active pending build #{build_num} with status '{b_status}' in existing builds list.", flush=True)
                        break

            if not build_num:
                try:
                    latest_builds, _ = make_request(f"{base_url}/builds?limit=10")
                    if isinstance(latest_builds, list):
                        for b in latest_builds:
                            if isinstance(b, dict):
                                b_status = b.get("buildStatus")
                                if b_status in active_statuses and b.get("build") is not None:
                                    build_num = b.get("build")
                                    existing_status = b_status
                                    print(f"Found active pending build #{build_num} with status '{b_status}' on re-query.", flush=True)
                                    break
                except Exception as e:
                    print(f"Notice re-querying builds: {e}", flush=True)

    except Exception as e:
        print(f"Notice/Exception triggering build via API: {e}", flush=True)
        for b in builds:
            if isinstance(b, dict):
                b_status = b.get("buildStatus")
                if b_status in active_statuses and b.get("build") is not None:
                    build_num = b.get("build")
                    existing_status = b_status
                    print(f"Found active pending build #{build_num} with status '{b_status}' after trigger exception.", flush=True)
                    break

if not build_num or str(build_num).strip().lower() == "none":
    raise RuntimeError("Cannot start build - already a build pending or invalid build_num (None). No active build (created/queued/sentToBuilder/building/started) found in /builds.")

if existing_status == "success":
    print(f"Build #{build_num} status is already 'success'. Skipping polling loop.")
    build_success = True
else:
    print(f"=== Polling Unity DevOps build #{build_num} status ===")
    max_wait_minutes = 90
    poll_interval = 30
    elapsed = 0
    build_success = False

    while not build_success and elapsed < max_wait_minutes * 60:
        try:
            b_info, _ = make_request(f"{base_url}/builds/{build_num}")
            status = b_info.get("buildStatus")
            print(f"Build #{build_num} status: '{status}' ({elapsed // 60}m {elapsed % 60}s elapsed)")

            if status == "success":
                print(f"Build #{build_num} completed successfully! Exiting polling loop immediately.")
                build_success = True
                break
            elif status in ["failure", "canceled"]:
                print(f"ERROR: Unity DevOps build #{build_num} failed with status '{status}'.")
                sys.exit(1)
        except Exception as e:
            print(f"Warning during polling: {e}")

        time.sleep(poll_interval)
        elapsed += poll_interval

if not build_success:
    print(f"ERROR: Timed out waiting for Unity DevOps build #{build_num} after {max_wait_minutes} minutes.")
    sys.exit(1)

print(f"=== Retrieving artifact info for build #{build_num} ===")

if not b_info:
    try:
        b_info, _ = make_request(f"{base_url}/builds/{build_num}")
        print(f"DEBUG: Complete JSON returned by GET /builds/{build_num}:\n{json.dumps(b_info, indent=2)}")
    except Exception as e:
        print(f"ERROR fetching GET /builds/{build_num}: {e}")
        sys.exit(1)
else:
    print(f"DEBUG: Using stored b_info from polling for build #{build_num}:\n{json.dumps(b_info, indent=2)}")

ipa_url, download_method = extract_download_info(b_info or {})

if not ipa_url:
    print("ERROR: Could not retrieve IPA download URL from completed Unity DevOps build.")
    sys.exit(1)

print(f"Resolved Artifact Download URL: {ipa_url} (Method: {download_method})")

print("=== Downloading build artifact ===")
download_path = "build_download.tmp"

download_success, resp_headers = download_artifact(ipa_url, download_method, download_path)

if not download_success:
    print("ERROR: Failed to download artifact from Unity DevOps.")
    sys.exit(1)

print("\n=== Inspecting downloaded build artifact ===")
file_size = os.path.getsize(download_path)
content_type = resp_headers.get("Content-Type", resp_headers.get("content-type", "Unknown"))
print(f"Downloaded File Path: {download_path} | Size: {file_size} bytes")
print(f"HTTP Response Content-Type: {content_type}")

with open(download_path, "rb") as f:
    first_bytes = f.read(32)
hex_bytes = " ".join(f"{b:02x}" for b in first_bytes)
ascii_bytes = "".join(chr(b) if 32 <= b <= 126 else "." for b in first_bytes)
print(f"First 32 bytes (Hex):   {hex_bytes}")
print(f"First 32 bytes (ASCII): {ascii_bytes}")

if zipfile.is_zipfile(download_path):
    print("\nDownloaded file is formatted as a ZIP archive. Inspecting entries...")
    with zipfile.ZipFile(download_path, "r") as zip_ref:
        namelist = zip_ref.namelist()
        print(f"Total entries in ZIP archive: {len(namelist)}")

        # Check if Payload/ folder exists inside (indicating this zip IS directly an .ipa file)
        has_payload = any(m.startswith("Payload/") or m.startswith("payload/") or "/Payload/" in m or "/payload/" in m for m in namelist)

        # Search for .ipa members (root or subdirectories)
        ipa_members = [m for m in namelist if m.lower().endswith(".ipa")]

        # Non-directory file members
        file_members = [m for m in namelist if not m.endswith("/")]

        if has_payload:
            print("ZIP archive contains 'Payload/' directory structure -> File IS ALREADY a valid .ipa package!")
            zip_ref.close()
            if os.path.exists("Unity-iPhone.ipa"):
                os.remove("Unity-iPhone.ipa")
            os.rename(download_path, "Unity-iPhone.ipa")
            print("Saved artifact directly as 'Unity-iPhone.ipa' without extraction.")
        elif ipa_members:
            chosen_ipa = ipa_members[0]
            print(f"Found embedded .ipa file in archive: '{chosen_ipa}'. Extracting...")
            zip_ref.extract(chosen_ipa, ".")
            zip_ref.close()
            if chosen_ipa != "Unity-iPhone.ipa":
                if os.path.exists("Unity-iPhone.ipa"):
                    os.remove("Unity-iPhone.ipa")
                os.rename(chosen_ipa, "Unity-iPhone.ipa")
            os.remove(download_path)
            print("Successfully extracted and saved 'Unity-iPhone.ipa'")
        elif len(file_members) == 1:
            single_file = file_members[0]
            print(f"ZIP archive contains a single file: '{single_file}'. Extracting as IPA...")
            zip_ref.extract(single_file, ".")
            zip_ref.close()
            if single_file != "Unity-iPhone.ipa":
                if os.path.exists("Unity-iPhone.ipa"):
                    os.remove("Unity-iPhone.ipa")
                os.rename(single_file, "Unity-iPhone.ipa")
            os.remove(download_path)
            print("Successfully extracted single file as 'Unity-iPhone.ipa'")
        else:
            print("\n=== ALL FILES IN ZIP ARCHIVE ===")
            for member in namelist:
                print(f" - {member}")
            print("================================")
            print("ERROR: No .ipa file or Payload/ directory found inside downloaded ZIP archive.")
            sys.exit(1)
else:
    print("\nDownloaded file is NOT a ZIP archive format. Saving directly as IPA...")
    if os.path.exists("Unity-iPhone.ipa"):
        os.remove("Unity-iPhone.ipa")
    os.rename(download_path, "Unity-iPhone.ipa")
    print("Saved artifact directly as 'Unity-iPhone.ipa'")

print("Download and artifact verification complete: 'Unity-iPhone.ipa'")



