#!/usr/local/bin/python3
import glob
import os
import re
import json

targetPath = "/virtual/yarukizero/public_html/futamaki/"
baseUrl = "https://dev.yarukizero.net/futamaki/"
releaseConf = "/virtual/yarukizero/futamaki/canary.version.json"
releaseUrl = "https://install.appcenter.ms/users/azumyar/apps/futamaki/distribution_groups/release"
canaryConf = "/virtual/yarukizero/futamaki/canary.version.json"
canaryUrl = "https://install.appcenter.ms/users/azumyar/apps/futamaki/distribution_groups/canary"

ret = {}
files = glob.glob(targetPath + "*.zip")
if 0 < len(files):
    files.sort(
        key=lambda x: int(os.path.getctime(x)),
        reverse=True)
    f = os.path.split(files[0])[1]
    m = re.match("^futamaki-0*(\\d+)\\.zip", f)
    p = None
    if m:
        p = int(m.group(1))
    ret = { "period": p, "file": f, "url": (baseUrl + f) }

f = open(releaseConf, "r")
js =json.load(f)
f.close()

#ret["release-version"] = js["version"]
#ret["release-url"] = releaseUrl

f = open(canaryConf, "r")
js =json.load(f)
f.close()

ret["canary-version"] = js["version"]
ret["canary-url"] = canaryUrl

print("Content-type: application/json; charset=UTF-8")
print("")
print(json.dumps(ret))
