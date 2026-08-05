import os
import sys
import zipfile
from typing import Dict, Tuple
from http_client import HTTPClient


def download_artifact(
    url: str, method: str, download_path: str, http_client: HTTPClient
) -> Tuple[bool, Dict[str, str]]:
    """Download artifact using HTTPClient."""
    return http_client.download_file(url, method, download_path)


def save_as_unity_iphone(source_path: str, target_name: str = "Unity-iPhone.ipa") -> None:
    """Safely replace target_name with source_path."""
    if os.path.exists(target_name):
        os.remove(target_name)
    os.rename(source_path, target_name)


def verify_and_process_ipa(download_path: str, target_name: str = "Unity-iPhone.ipa") -> None:
    """
    Verify downloaded artifact structure and format.
    - If ZIP contains 'Payload/' folder -> file IS directly an IPA package (no extraction).
    - If ZIP contains an embedded '.ipa' file -> extract and rename.
    - If ZIP contains a single file -> extract as IPA.
    - If neither Payload/ nor .ipa -> log zip entries and exit with error.
    """
    if not os.path.exists(download_path):
        print(f"ERROR: Downloaded file '{download_path}' not found.", flush=True)
        sys.exit(1)

    if zipfile.is_zipfile(download_path):
        with zipfile.ZipFile(download_path, "r") as zip_ref:
            namelist = zip_ref.namelist()

            has_payload = any(
                m.startswith("Payload/") or m.startswith("payload/") or "/Payload/" in m or "/payload/" in m
                for m in namelist
            )
            ipa_members = [m for m in namelist if m.lower().endswith(".ipa")]
            file_members = [m for m in namelist if not m.endswith("/")]

            if has_payload:
                zip_ref.close()
                save_as_unity_iphone(download_path, target_name)
            elif ipa_members:
                chosen_ipa = ipa_members[0]
                zip_ref.extract(chosen_ipa, ".")
                zip_ref.close()
                if chosen_ipa != target_name:
                    save_as_unity_iphone(chosen_ipa, target_name)
                os.remove(download_path)
            elif len(file_members) == 1:
                single_file = file_members[0]
                zip_ref.extract(single_file, ".")
                zip_ref.close()
                if single_file != target_name:
                    save_as_unity_iphone(single_file, target_name)
                os.remove(download_path)
            else:
                print("\n=== ZIP CONTENTS ===", flush=True)
                for m in namelist:
                    print(f" - {m}", flush=True)
                print("====================", flush=True)
                print("ERROR: No .ipa file or Payload/ directory found inside downloaded ZIP archive.", flush=True)
                sys.exit(1)
    else:
        save_as_unity_iphone(download_path, target_name)

    print("IPA verified.", flush=True)
    print(f"Saved as {target_name}.", flush=True)
