#!/usr/local/bin/python3
import glob
import os
import json

targetPath = "/virtual/yarukizero/public_html/futamaki/"
baseUrl = "https://dev.yarukizero.net/futamaki/"

ret = {}
files = glob.glob(targetPath + "*.zip")
if 0 < len(files):
    files.sort(
        key=lambda x: int(os.path.getctime(x)),
        reverse=True)
    f = os.path.split(files[0])[1]
    ret = { "file": f, "url": (baseUrl + f) }

print("Content-type: application/json; charset=UTF-8")
print('')
print(json.dumps(ret))