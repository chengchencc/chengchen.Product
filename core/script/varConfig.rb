PROJECT_NAME = "BlackMamba Framework"

# the variable used in the ruby files
ROOT = File.expand_path(File.join(File.dirname(__FILE__), ".."))
puts "Project root directory is " + ROOT

SOURCE_ROOT=File.join(ROOT, "src")
# Webdevexe does not accept "/" as a directory path, use "\\" instead

ROOT_PARENT=File.join(ROOT, "..")
SOLUTION = File.join(SOURCE_ROOT,"BlackMamba.sln")
XUNIT_CONFIG_NAME = "core.xunit"
XUNIT_CONFIG= File.join(ROOT, "script/"+XUNIT_CONFIG_NAME)
XUNIT_PATH = File.join(ROOT, "lib/xUnit/xunit.console.clr4.exe")
ZIP_EXE = File.join(ROOT, "lib/7Zip/64/7z.exe")



if ((defined? ENV) && ENV.to_hash.has_key?("compile.mode")) then
	COMPILE_MODE = ENV['compile.mode']
else
	COMPILE_MODE = "Release"
end	

OUTPUT_DIRECTORY = File.join(ROOT, "/bin")	
OUTPUT_DLL_DIR = File.join(OUTPUT_DIRECTORY, COMPILE_MODE)
PUBLISH_DIR = File.join(OUTPUT_DIRECTORY, "Publish")

WEB_DEV_NAME = "WebDev.WebServer40.EXE"
NCOVER_CONSOLE= "C:/Program Files/NCover/NCover.Console.exe"
NCOVER_REPORTING =  "C:/Program Files/NCover/NCover.Reporting.exe"
DEPLOY_SOURCE = File.join(PUBLISH_DIR, COMPILE_MODE)
CHECK_UTF8_EXE=File.join(ROOT, "lib/CheckUTF8/SearchNonUTF8Files.exe")

COMMON_ASSEMBLY=File.join(ROOT, "src/COMMON/CommonAssemblyInfo.cs")

SNPATH="C:/Program Files (x86)/Microsoft SDKs/Windows/v8.0A/bin/NETFX 4.0 Tools/x64/sn.exe"
