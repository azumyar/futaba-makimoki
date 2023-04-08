#!/usr/local/bin/python3
import glob
import os
import re
import json

# version.jsonのapp-center-versionは今の時点で入ってないので今後実装

targetPath = "/virtual/yarukizero/public_html/futamaki/"
baseUrl = "https://dev.yarukizero.net/futamaki/"
releaseConf = "/virtual/yarukizero/futamaki/release.version.json"
releaseUrl = "https://install.appcenter.ms/users/azumyar/apps/futamaki/distribution_groups/release"
canaryConf = "/virtual/yarukizero/futamaki/canary.version.json"
canaryUrl = "https://install.appcenter.ms/users/azumyar/apps/futamaki/distribution_groups/canary"

ret = {}
f = open(releaseConf, "r")
js =json.load(f)
f.close()

m = re.match("^\\d+\.(\\d+)\\..*$", js["version"])
p = None
if m:
    p = int(m.group(1))

ret["period"] = p
ret["file"] = None
ret["url"] = releaseUrl
ret["release-version"] = js["version"]
ret["release-app-center-version"] = js["app-center-version"]
ret["release-url"] = releaseUrl

f = open(canaryConf, "r")
js =json.load(f)
f.close()

ret["canary-version"] = js["version"]
ret["canary-app-center-version"] = js["app-center-version"]
ret["canary-url"] = canaryUrl

print("Content-type: application/json; charset=UTF-8")
print("")
print(json.dumps(ret))
