import time
import sys
from typing import Any, Dict, List, Optional, Tuple
from http_client import HTTPClient


class UnityDevOpsAPI:
    """Encapsulates Unity DevOps Build Automation API operations."""

    ACTIVE_STATUSES = ["created", "queued", "sentToBuilder", "building", "started"]

    def __init__(self, http_client: HTTPClient, org_id: str, project_id: str, target_id: str):
        self.http_client = http_client
        self.org_id = org_id.strip()
        self.project_id = project_id.strip()
        self.target_id = target_id.strip()
        self.base_url = (
            f"https://build-api.cloud.unity3d.com/api/v1"
            f"/orgs/{self.org_id}/projects/{self.project_id}/buildtargets/{self.target_id}"
        )

    def find_active_or_existing_build(self, commit_sha: str) -> Tuple[Optional[int], Optional[str]]:
        """Query builds list to find existing completed or running build for commit_sha."""
        try:
            builds_resp, _ = self.http_client.request(f"{self.base_url}/builds?limit=10")
            print("=== BUILDS ===", flush=True)
            print(builds_resp, flush=True)
            print("==============", flush=True)
            builds: List[Dict[str, Any]] = []
            if isinstance(builds_resp, list):
                builds = builds_resp
            elif isinstance(builds_resp, dict) and "builds" in builds_resp:
                builds = builds_resp.get("builds", [])

            for b in builds:
                if not isinstance(b, dict):
                    continue
                if b.get("lastBuiltRevision") == commit_sha or b.get("commit") == commit_sha:
                    b_status = b.get("buildStatus")
                    if b_status in self.ACTIVE_STATUSES + ["success"]:
                        build_num = b.get("build")
                        return build_num, b_status
        except Exception:
            pass
        return None, None

    def trigger_build(self, commit_sha: str) -> Tuple[Optional[int], Optional[str]]:
        """Trigger a new Unity DevOps build via POST /builds."""
        existing_num, existing_status = self.find_active_or_existing_build(commit_sha)
        if existing_num:
            return existing_num, existing_status

        trigger_data = {
            "clean": False,
            "commit": commit_sha,
            "label": f"GitHub Actions {commit_sha[:7]}" if commit_sha else "GitHub Actions",
        }

        try:
            resp, _ = self.http_client.request(f"{self.base_url}/builds", method="POST", data=trigger_data)
            build_num = None

            if isinstance(resp, list) and len(resp) > 0 and isinstance(resp[0], dict):
                build_num = resp[0].get("build")
            elif isinstance(resp, dict):
                build_num = resp.get("build")

            if build_num is not None:
                return build_num, "queued"
        except Exception:
            pass

        # Fallback query if build trigger returned error due to pending build
        return self.find_active_or_existing_build(commit_sha)

    def get_build(self, build_num: int) -> Dict[str, Any]:
        """Fetch details for a specific build ID."""
        b_info, _ = self.http_client.request(f"{self.base_url}/builds/{build_num}")
        return b_info if isinstance(b_info, dict) else {}

    def wait_until_finished(
        self, build_num: int, max_wait_minutes: int = 90, poll_interval: int = 30
    ) -> Dict[str, Any]:
        """Poll build status until completed."""
        elapsed = 0
        last_reported_status: Optional[str] = None

        while elapsed < max_wait_minutes * 60:
            b_info = self.get_build(build_num)
            status = b_info.get("buildStatus")

            if status != last_reported_status:
                print(f"Status: {status}", flush=True)
                last_reported_status = status

            if status == "success":
                return b_info
            elif status in ["failure", "canceled"]:
                print(f"ERROR: Unity DevOps build #{build_num} failed with status '{status}'.", flush=True)
                sys.exit(1)

            time.sleep(poll_interval)
            elapsed += poll_interval

        print(f"ERROR: Timed out waiting for build #{build_num} after {max_wait_minutes} minutes.", flush=True)
        sys.exit(1)

    @staticmethod
    def get_download_url(b_info: Dict[str, Any]) -> Tuple[str, str]:
        """Extract download_primary URL from build info links."""
        links = b_info.get("links", {}) if isinstance(b_info, dict) else {}

        # 1. Primary link requirement: build["links"]["download_primary"]["href"]
        for key in ["download_primary", "download"]:
            if key in links:
                obj = links[key]
                if isinstance(obj, str):
                    url = obj
                    method = "GET"
                elif isinstance(obj, dict):
                    url = obj.get("href") or obj.get("url") or obj.get("download_url")
                    method = obj.get("method", "GET").upper()
                else:
                    url, method = None, "GET"

                if url:
                    if url.startswith("/"):
                        url = f"https://build-api.cloud.unity3d.com{url}"
                    return url, method

        # Fallback to artifacts field in b_info
        b_artifacts = b_info.get("artifacts", [])
        if isinstance(b_artifacts, list):
            for art in b_artifacts:
                if not isinstance(art, dict):
                    continue
                files = art.get("files", [])
                if isinstance(files, list):
                    for f in files:
                        if isinstance(f, dict) and f.get("href"):
                            url = f["href"]
                            if url.startswith("/"):
                                url = f"https://build-api.cloud.unity3d.com{url}"
                            return url, "GET"

        print("ERROR: Could not find download_primary URL in build info.", flush=True)
        sys.exit(1)
