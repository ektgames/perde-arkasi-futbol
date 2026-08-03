import os
import sys
import time
import json
import urllib.request
import urllib.error
import base64
import zipfile

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

# Redirect handler that strips Auth header for external cloud storage S3/GCS redirects
class StripAuthRedirectHandler(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        new_req = super().redirect_request(req, fp, code, msg, headers, newurl)
        if new_req and ("s3.amazonaws.com" in newurl or "storage.googleapis.com" in newurl or "cloudfront.net" in newurl):
            if new_req.has_header("Authorization"):
                new_req.remove_header("Authorization")
        return new_req

opener = urllib.request.build_opener(StripAuthRedirectHandler())

def make_request(url, method="GET", data=None):
    last_err = None
    for auth_hdr in auth_headers:
        req = urllib.request.Request(url, method=method)
        req.add_header("Authorization", auth_hdr)
        req.add_header("Content-Type", "application/json")
        if data:
            req.data = json.dumps(data).encode("utf-8")
        try:
            with opener.open(req) as resp:
                return json.loads(resp.read().decode("utf-8")), resp.status
        except urllib.error.HTTPError as e:
            last_err = e
            if e.code == 401:
                continue
            body = e.read().decode("utf-8", errors="ignore")
            print(f"HTTP Error {e.code}: {body}")
            raise e
    print("HTTP Authentication Error: Could not authenticate with provided UNITY_DEVOPS_API_KEY.")
    raise last_err

print(f"=== Checking existing Unity DevOps builds for target '{target_id}' ===")
build_num = None

try:
    builds, _ = make_request(f"{base_url}/builds?limit=10")
    for b in builds:
        if b.get("lastBuiltRevision") == commit_sha or b.get("commit") == commit_sha:
            b_status = b.get("buildStatus")
            if b_status in ["queued", "started", "success"]:
                build_num = b.get("build")
                print(f"Found existing build #{build_num} with status '{b_status}' for commit {commit_sha}")
                break
except Exception as e:
    print(f"Notice querying builds: {e}")

if not build_num:
    print(f"Triggering new Unity DevOps build for target '{target_id}' on commit {commit_sha}...")
    trigger_data = {"clean": False, "commit": commit_sha, "label": f"GitHub Actions {commit_sha[:7]}"}
    try:
        resp, _ = make_request(f"{base_url}/builds", method="POST", data=trigger_data)
        if isinstance(resp, list) and len(resp) > 0:
            build_num = resp[0].get("build")
        elif isinstance(resp, dict):
            build_num = resp.get("build")
        print(f"Build #{build_num} successfully queued on Unity DevOps!")
    except Exception as e:
        print(f"Failed to trigger build via API: {e}")
        sys.exit(1)

print(f"=== Polling Unity DevOps build #{build_num} status ===")
max_wait_minutes = 90
poll_interval = 30
elapsed = 0
ipa_url = None

while elapsed < max_wait_minutes * 60:
    try:
        b_info, _ = make_request(f"{base_url}/builds/{build_num}")
        status = b_info.get("buildStatus")
        print(f"Build #{build_num} status: '{status}' ({elapsed // 60}m {elapsed % 60}s elapsed)")

        if status == "success":
            links = b_info.get("links", {})
            if "download" in links:
                ipa_url = links["download"].get("href")

            if not ipa_url:
                artifacts, _ = make_request(f"{base_url}/builds/{build_num}/artifacts")
                for art in artifacts:
                    for file_info in art.get("files", []):
                        fn = file_info.get("filename", "")
                        if fn.endswith(".ipa") or file_info.get("name", "").endswith(".ipa"):
                            ipa_url = file_info.get("href")
                            break
                    if ipa_url:
                        break
            break
        elif status in ["failure", "canceled"]:
            print(f"ERROR: Unity DevOps build #{build_num} failed with status '{status}'.")
            sys.exit(1)
    except Exception as e:
        print(f"Warning during polling: {e}")

    time.sleep(poll_interval)
    elapsed += poll_interval

if not ipa_url:
    print("ERROR: Could not retrieve IPA download URL from completed Unity DevOps build.")
    sys.exit(1)

print("=== Downloading build artifact ===")
download_path = "build_download.tmp"
download_success = False

for auth_hdr in auth_headers + [None]:
    req = urllib.request.Request(ipa_url)
    if auth_hdr:
        req.add_header("Authorization", auth_hdr)
    try:
        with opener.open(req) as resp, open(download_path, "wb") as out_file:
            out_file.write(resp.read())
        download_success = True
        break
    except urllib.error.HTTPError:
        continue

if not download_success:
    print("ERROR: Failed to download artifact from Unity DevOps.")
    sys.exit(1)

if zipfile.is_zipfile(download_path):
    print("Downloaded file is a ZIP archive, extracting .ipa...")
    with zipfile.ZipFile(download_path, "r") as zip_ref:
        ipa_members = [m for m in zip_ref.namelist() if m.endswith(".ipa")]
        if ipa_members:
            zip_ref.extract(ipa_members[0], ".")
            if ipa_members[0] != "Unity-iPhone.ipa":
                os.rename(ipa_members[0], "Unity-iPhone.ipa")
        else:
            print("ERROR: No .ipa file found inside downloaded ZIP archive.")
            sys.exit(1)
    os.remove(download_path)
else:
    os.rename(download_path, "Unity-iPhone.ipa")

print("Download and extraction complete: 'Unity-iPhone.ipa'")
