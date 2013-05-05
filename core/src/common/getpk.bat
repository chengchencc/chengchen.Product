SET sn=C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\sn.exe
"%sn%" -p core.snk core.PublicKey
"%sn%" -tp core.PublicKey
PAUSE