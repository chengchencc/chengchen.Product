# the variable used in the ruby files
ROOT = File.expand_path(File.join(File.dirname(__FILE__), ".."))
puts "Project root directory is " + ROOT

#load File.join(ROOT, '/core/commonRake.rb')
SOURCE_ROOT=File.join(ROOT, "src")
SCRIPT_ROOT=File.join(ROOT, "script")

# custome values
SOLUTION_NAME="BlackMamba.Billing.sln"
XUNIT_CONFIG_NAME = "billing.xunit"
WEBSITE_PATH = "BlackMamba.Billing.Website"
WEBSITE_FILE_NAME = "BlackMamba.Billing.Website.csproj"


# Webdevexe does not accept "/" as a directory path, use "\\" instead
SOLUTION = File.join(SOURCE_ROOT,SOLUTION_NAME)

WEBSITE_LOCATION = File.expand_path(File.join(SOURCE_ROOT, WEBSITE_PATH)).gsub(/\//, "\\")
WEBSITE_PROJ = File.join(WEBSITE_LOCATION,WEBSITE_FILE_NAME)
XUNIT_CONFIG= File.join(SCRIPT_ROOT, XUNIT_CONFIG_NAME)
CORE_MODULE=File.join(ROOT, "core")

if FileTest::directory?(CORE_MODULE) then
	TOOL_ROOT=File.join(CORE_MODULE, "lib")
end
OUTPUT_DIRECTORY = File.join(ROOT, "/bin")


# Tools part
UPDATE_SUBMODOLUE_BAT=File.join(TOOL_ROOT, "/batch/updatesubmodule.bat")
XUNIT_PATH = File.join(TOOL_ROOT, "/xUnit/xunit.console.clr4.exe")
ZIP_EXE = File.join(TOOL_ROOT, "/7Zip/64/7z.exe")
CHECK_UTF8_EXE=File.join(TOOL_ROOT, "/CheckUTF8/SearchNonUTF8Files.exe")
NCOVER_CONSOLE= "C:/Program Files/NCover/NCover.Console.exe"
NCOVER_REPORTING="C:/Program Files/NCover/NCover.Reporting.exe"
PROJECT_NAME = "BlackMambaBilling"
#WEB_DEV_NAME = "WebDev.WebServer40.EXE"
#WARMUP_EXE = File.join(ROOT, "/Warmup/Warmup.exe")
#UPLOAD_BAT=File.join(SOURCE_ROOT,"script/uploaddaily.bat")
#CHURN_PATH = File.join(ROOT, "lib/nchurn/nchurn.exe")

#### compile mode
if ((defined? ENV) && ENV.to_hash.has_key?("compile.mode")) then
	COMPILE_MODE = ENV['compile.mode']
else
	COMPILE_MODE = "Debug"
end	
	
OUTPUT_DLL_DIR = File.join(OUTPUT_DIRECTORY, COMPILE_MODE)
OUTPUT_SERVICE_DIR = File.join(ROOT, "WinProgram/" + COMPILE_MODE)
PUBLISH_DIR = File.join(OUTPUT_DIRECTORY, "Publish")
#OUTPUT_CHURN_DIR = File.join(OUTPUT_DLL_DIR, "Churn")
#OUTPUT_CHURN_FILE = File.join(OUTPUT_CHURN_DIR, "churn.txt")

#WEB_DEV_FULL_NAME = "C:/Program Files/Common Files/Microsoft Shared/DevServer/10.0/" + WEB_DEV_NAME

DEPLOY_SOURCE = File.join(PUBLISH_DIR, COMPILE_MODE)
PUBLISH_PARAMETER_FILE = File.join(DEPLOY_SOURCE, "BlackMamba.Billing.Website.SetParameters.xml")
ROOT_PARENT=File.join(ROOT, "..")

COMMON_ASSEMBLY=File.join(ROOT, "src/COMMON/CommonAssemblyInfo.cs")


WEBSERVER_ROOT = "C:/inetpub/wwwroot/Billing"
WIN_LOGGING_ROOT = File.join(OUTPUT_SERVICE_DIR, "BillingService")

#WARMUP_INFRA_URL="http://itest.kk570.com/wallpaper/categorylist?imsi=348343^&istest=1"
#REST_SERVICESERVER_ROOT = "C:/inetpub/wwwroot/"
SNPATH="C:/Program Files (x86)/Microsoft SDKs/Windows/v8.0A/bin/NETFX 4.0 Tools/x64/sn.exe"

#F_ROOT=File.join(ROOT,"bin/fuscated")
#OBSCURE=File.join(ROOT, "script/obscure.bat")
#OBSCUREREPLACE=File.join(ROOT, "script/obscurereplace.bat")