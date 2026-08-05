import os
import sys
from http_client import HTTPClient
from unity_api import UnityDevOpsAPI
from ipa_utils import download_artifact, verify_and_process_ipa


def main() -> None:
    api_key = os.environ.get("UNITY_DEVOPS_API_KEY", "").strip()
    org_id = os.environ.get("UNITY_ORG_ID", "").strip()
    project_id = os.environ.get("UNITY_PROJECT_ID", "").strip()
    target_id = os.environ.get("UNITY_BUILD_TARGET_ID", "").strip() or "ios"
    commit_sha = os.environ.get("GITHUB_SHA", "").strip()

    if not api_key or not org_id or not project_id:
        print(
            "ERROR: Missing required secrets! Please configure UNITY_DEVOPS_API_KEY, "
            "UNITY_ORG_ID, and UNITY_PROJECT_ID in GitHub Repository Secrets.",
            flush=True,
        )
        sys.exit(1)

    print("Starting Unity build...", flush=True)

    http_client = HTTPClient(api_key=api_key)
    api = UnityDevOpsAPI(
        http_client=http_client,
        org_id=org_id,
        project_id=project_id,
        target_id=target_id,
    )

    build_num, initial_status = api.trigger_build(commit_sha=commit_sha)

    if not build_num:
        print("ERROR: Could not trigger or locate build on Unity DevOps.", flush=True)
        sys.exit(1)

    print(f"Build ID: {build_num}", flush=True)

    if initial_status == "success":
        print("Status: success", flush=True)
        b_info = api.get_build(build_num)
    else:
        b_info = api.wait_until_finished(build_num=build_num)

    download_url, download_method = api.get_download_url(b_info)

    print("Downloading IPA...", flush=True)
    temp_download_path = "build_download.tmp"
    success, _ = download_artifact(
        url=download_url,
        method=download_method,
        download_path=temp_download_path,
        http_client=http_client,
    )

    if not success:
        print("ERROR: Failed to download artifact from Unity DevOps.", flush=True)
        sys.exit(1)

    verify_and_process_ipa(download_path=temp_download_path, target_name="Unity-iPhone.ipa")
    print("Done.", flush=True)


if __name__ == "__main__":
    main()
