SET sn=C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\sn.exe
"%sn%" -p billing.snk billing.PublicKey
"%sn%" -tp billing.PublicKey
PAUSE