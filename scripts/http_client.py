import base64
import json
import os
import socket
import sys
import urllib.error
import urllib.parse
import urllib.request
from typing import Any, Dict, List, Optional, Tuple


class StripAuthRedirectHandler(urllib.request.HTTPRedirectHandler):
    """Redirect handler that strips Authorization headers when redirected to third-party domains (e.g. S3, GCS)."""

    def redirect_request(
        self, req: urllib.request.Request, fp: Any, code: int, msg: str, headers: Any, newurl: str
    ) -> Optional[urllib.request.Request]:
        new_req = super().redirect_request(req, fp, code, msg, headers, newurl)
        if new_req:
            parsed_new = urllib.parse.urlparse(newurl)
            parsed_req = urllib.parse.urlparse(req.full_url)
            cloud_domains = ["s3", "amazonaws", "googleapis", "cloudfront", "storage"]
            if parsed_new.netloc != parsed_req.netloc or any(domain in newurl.lower() for domain in cloud_domains):
                if new_req.has_header("Authorization"):
                    new_req.remove_header("Authorization")
                if new_req.has_header("authorization"):
                    new_req.remove_header("authorization")
        return new_req


class HTTPClient:
    """HTTP Client with authentication fallback and redirect authorization stripping."""

    def __init__(self, api_key: str):
        self.api_key = api_key.strip()
        basic_b64 = base64.b64encode(f"{self.api_key}:".encode("utf-8")).decode("utf-8")
        self.auth_headers: List[str] = [
            f"Basic {self.api_key}",
            f"Basic {basic_b64}",
            f"Bearer {self.api_key}",
        ]
        self.working_auth_hdr: Optional[str] = None
        self.opener = urllib.request.build_opener(StripAuthRedirectHandler())

    def _get_headers_to_try(self) -> List[Optional[str]]:
        if self.working_auth_hdr:
            return [self.working_auth_hdr] + [h for h in self.auth_headers if h != self.working_auth_hdr]
        return list(self.auth_headers)

    def request(
        self, url: str, method: str = "GET", data: Optional[Dict[str, Any]] = None
    ) -> Tuple[Any, int]:
        """Perform an HTTP JSON request using Authorization headers."""
        headers_to_try = self._get_headers_to_try()
        last_err: Optional[Exception] = None

        for auth_hdr in headers_to_try:
            req = urllib.request.Request(url, method=method)
            if auth_hdr:
                req.add_header("Authorization", auth_hdr)
            req.add_header("Accept", "application/json")
            if data:
                req.add_header("Content-Type", "application/json")
                req.data = json.dumps(data).encode("utf-8")

            try:
                with self.opener.open(req, timeout=30) as resp:
                    body_bytes = resp.read()
                    body_text = body_bytes.decode("utf-8", errors="ignore")
                    self.working_auth_hdr = auth_hdr
                    parsed_json = json.loads(body_text) if body_text.strip() else {}
                    return parsed_json, resp.status
            except (socket.timeout, urllib.error.URLError) as e:
                if isinstance(e, socket.timeout) or (hasattr(e, "reason") and isinstance(e.reason, socket.timeout)):
                    print(f"HTTP TIMEOUT for {url}", flush=True)
                    sys.exit(1)
                if isinstance(e, urllib.error.HTTPError):
                    last_err = e
                    if e.code == 429:
                        print(f"HTTP 429 Too Many Requests for {url}", flush=True)
                        sys.exit(1)
                    if e.code == 401:
                        continue
                    body_text = e.read().decode("utf-8", errors="ignore")
                    raise RuntimeError(f"HTTP Error {e.code} for {url}:\n{body_text}") from e
                raise RuntimeError(f"URL request failed for {url}: {e}") from e

        if last_err is not None:
            raise RuntimeError(f"HTTP Auth Error for {url}. Last error: {last_err}") from last_err
        raise RuntimeError(f"HTTP Auth Error for {url}.")

    def download_file(
        self, url: str, method: str, download_path: str
    ) -> Tuple[bool, Dict[str, str]]:
        """Download binary artifact to file."""
        parsed = urllib.parse.urlparse(url)
        is_unity_api = "build-api.cloud.unity3d.com" in parsed.netloc or url.startswith("/")

        if is_unity_api:
            hdrs_to_try: List[Optional[str]] = (
                ([self.working_auth_hdr] if self.working_auth_hdr else [])
                + [h for h in self.auth_headers if h != self.working_auth_hdr]
                + [None]
            )
        else:
            hdrs_to_try = (
                [None]
                + ([self.working_auth_hdr] if self.working_auth_hdr else [])
                + [h for h in self.auth_headers if h != self.working_auth_hdr]
            )

        for auth_hdr in hdrs_to_try:
            req = urllib.request.Request(url, method=method)
            if auth_hdr:
                req.add_header("Authorization", auth_hdr)
            req.add_header("Accept", "*/*")

            try:
                with self.opener.open(req, timeout=60) as resp:
                    resp_headers = dict(resp.headers)
                    with open(download_path, "wb") as out_file:
                        out_file.write(resp.read())
                    return True, resp_headers
            except (socket.timeout, urllib.error.URLError) as e:
                if isinstance(e, socket.timeout) or (hasattr(e, "reason") and isinstance(e.reason, socket.timeout)):
                    print(f"HTTP TIMEOUT downloading {url}", flush=True)
                    sys.exit(1)
                continue
            except urllib.error.HTTPError as e:
                if e.code == 429:
                    print("HTTP 429 Too Many Requests during download.", flush=True)
                    sys.exit(1)
                continue
            except Exception:
                continue

        return False, {}
