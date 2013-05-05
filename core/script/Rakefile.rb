require  "albacore"
require 'fileutils'

###############################
if not((defined? ROOT)) then
	load File.join(File.dirname(__FILE__), 'varConfig.rb')
end
###############################

desc "initialize the server environment"
task :initialize do
	puts "delete the bin directory before building"
	if FileTest::directory?(OUTPUT_DLL_DIR) then
		FileUtils.rm_rf OUTPUT_DLL_DIR
	end
  
	#get the current version
	if ((defined? ENV) && ENV.to_hash.has_key?("BUILD_NUMBER")) then
		CURRENT_VERSION = ENV['BUILD_NUMBER']
	else	
		CURRENT_VERSION = '1.0.0.0'
	end
	
	puts "Current version is: #{CURRENT_VERSION}"
end

desc "Add Assembly Info"
assemblyinfo :addAsm=>:initialize do |asm|
	FileUtils.chmod 1777, COMMON_ASSEMBLY
	asm.input_file=COMMON_ASSEMBLY
	asm.version=CURRENT_VERSION
	asm.file_version=CURRENT_VERSION
	asm.output_file=COMMON_ASSEMBLY
end

desc "Build the solution"
msbuild :compile, :mode do |msb, args|
	args.with_defaults(:mode => COMPILE_MODE)
	
	msb.solution = SOLUTION
	msb.verbosity = "minimal"
	msb.targets :clean, :build
	msb.parameters  ["/P:Configuration=" + args[:mode]]
end

desc "Copy the xunit configuration file to working directory"
task :copyXunitConfig do
	newName = File.join(OUTPUT_DLL_DIR, XUNIT_CONFIG_NAME)
	FileUtils.copy_file(XUNIT_CONFIG, newName)
end

desc "run xunit"
xunit :xunit => :copyXunitConfig do |xunit|
	puts OUTPUT_DLL_DIR
    xunit.command = XUNIT_PATH
	xunit.assemblies = [
		File.join(OUTPUT_DLL_DIR, "BlackMamba.Framework.Tests.dll"),
		File.join(OUTPUT_DLL_DIR, "BlackMamba.Framework.Automation.Tests.dll")
		]
end

desc "Run a sample NCover Console code coverage"
ncoverconsole :coverage=>:copyXunitConfig do |ncc|
  ncc.command = NCOVER_CONSOLE
  ncc.output :xml => "CodeCoverage/coverage.xml",:p=>"FrameworkCore", :coverall=>"",:bi=>"#{CURRENT_VERSION}",:et=>".*\.program;.*installer;.*Fake;.*_DisplayClass.*?;.*AnonymousType.*?;.*Test;.*Tests;"
  ncc.working_directory = OUTPUT_DLL_DIR
  ncc.cover_assemblies "NCore.*", "N.*?"
  ncc.exclude_assemblies '.*test.*','.*dataaccess'
  
  xunit = XUnitTestRunner.new(XUNIT_PATH)
  xunit.options XUNIT_CONFIG_NAME+' /teamcity'

  ncc.testrunner = xunit
end


desc "Run a sample NCover Report to check code coverage"
ncoverreport :coveragereport =>:coverage do |ncr|
  ncr.command = NCOVER_REPORTING
  ncr.coverage_files File.join(OUTPUT_DLL_DIR, "CodeCoverage/coverage.xml")
		
  fullcoveragereport = NCover::FullCoverageReport.new
  fullcoveragereport.output_path = File.join(OUTPUT_DLL_DIR, "CodeCoverage/report")
  ncr.reports fullcoveragereport
		
  #ncr.required_coverage(
   # NCover::SymbolCoverage.new(:minimum => 1),
    #NCover::BranchCoverage.new(:minimum => 1, :item_type => :Class),
    #NCover::MethodCoverage.new(:minimum => 1)
  #)

  #ncr.required_coverage NCover::CyclomaticComplexity.new(:maximum => 100, :item_type => :Class)
end

desc "Check your repository if there is non-utf8 files"
task :checkUtf8Files do
	ret = system("call #{CHECK_UTF8_EXE} -d:#{ROOT_PARENT} -x:.cs,.cshtml,.aspx,.rb,.asax,.ashx,.ascx,.sql")
	raise if $?.exitstatus>0 
end

REXML::Attribute.class_eval( %q^
		def to_string
 			%Q[#@expanded_name="#{to_s().gsub(/"/, '&quot;')}"]
 		end
 	^ )

task :sign do
	puts "signing..."
	system("call s.bat #{COMPILE_MODE}")
end

task :fuscator do
	puts "empty"
end

task :teamcity => [:initialize, :compile, :coveragereport]

task :run => [:initialize, :compile, :fuscator, :xunit]

