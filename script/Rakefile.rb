require  "albacore"
require 'fileutils'


######### PreCheck ###########
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
    xunit.command = XUNIT_PATH
	xunit.assemblies = [
		File.join(OUTPUT_DLL_DIR, "BlackMamba.Billing.Tests.dll")
		]
end

desc "Run a sample NCover Console code coverage"
ncoverconsole :coverage=>:copyXunitConfig do |ncc|
puts NCOVER_CONSOLE
puts OUTPUT_DLL_DIR

  ncc.command = NCOVER_CONSOLE
  ncc.output :xml => "CodeCoverage/coverage.xml",:p=>"BlackMambaBilling", :coverall=>"",:bi=>"#{CURRENT_VERSION}",:et=>".*\.program;.*installer;.*Fake;.*_DisplayClass.*?;.*AnonymousType.*?;.*Test;.*Tests;Fake.*"
  ncc.working_directory = OUTPUT_DLL_DIR
  ncc.cover_assemblies "BlackMamba.*;"
  ncc.exclude_assemblies '.*test','.*tests','.*dataaccess', '.*PbModels'
  
  xunit = XUnitTestRunner.new(XUNIT_PATH)
  xunit.options XUNIT_CONFIG_NAME+' /teamcity'

  ncc.testrunner = xunit
end


desc "Run a sample NCover Report to check code coverage"
ncoverreport :coveragereport =>[:coverage] do |ncr|
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

desc 'starting to build deployment package for the website project'
msbuild :buildDeployPackage do |msb|
  puts "building deployment package"
  msb.solution = WEBSITE_PROJ
  msb.verbosity = "Quiet"
  msb.targets :publish
  msb.parameters  [ "/T:Package",
					"/P:Configuration=" + COMPILE_MODE]
end

task :deployLocal=>[:buildDeployPackage] do
	deploy_script_path = File.join(DEPLOY_SOURCE, "BlackMamba.billing.Website.deploy.cmd")
	system("'"+ deploy_script_path + "'" + " /Y")
end

task :deployDaily=>[:buildDeployPackage,:modifyParameterToDaily] do
	deploy_script_path = File.join(DEPLOY_SOURCE, "BlackMamba.billing.Website.deploy.cmd")
	system("'"+ deploy_script_path + "'" + " /Y")
end

task :deployRemote=>:buildDeployPackage do
	deploy_script_path = File.join(DEPLOY_SOURCE, "BlackMamba.billing.Website.deploy.cmd")
	remote_url=ENV['targetMachineUrl']
	remote_user=ENV['targetMachineUserName']
	remote_pass=ENV['targetMachinePassword']

	system("'"+ deploy_script_path + "'" + " /Y /M:#{remote_url} /U:#{remote_user} /P:#{remote_pass} /A:Basic -allowUntrusted")
end

task :compress, :name do |t,args|
	name = args.name
	puts "Compress in mode : #{name}"
	system("call "+ File.join(File.dirname(__FILE__), 'compress.bat '+name))
end

task :compressWithMode do
	Rake::Task[:compress].invoke(COMPILE_MODE)
end

task :compressDaily do
	Rake::Task[:compress].invoke('daily')
	#Rake.application.invoke_task("compress[daily]")
end

task :compressDebug do
	Rake::Task[:compress].invoke('debug')
	#Rake.application.invoke_task("compress[daily]")
end

desc "Added version to script and css"
task :AddVerToResource do
	#does not support daily currently
	layoutPage = File.join(WEBSERVER_ROOT, "Views/Shared/_AppStoreWapLayout.cshtml")
		
	data = ''
	f = File.open(layoutPage, "r:utf-8") 
	f.each_line do |line|
		data += line
	end

	#var version = "?ver=";
	data.gsub!(/(\?ver=)/, '\1'+CURRENT_VERSION)
	
	File.open(layoutPage, 'w:utf-8'){|f| f.write data}
end


task :zipWeb do
	# check the directory exists or not
	packageDir = File.join(OUTPUT_DLL_DIR, "Packages")
	if !FileTest::directory?(packageDir) then
		Dir.mkdir(packageDir)
	end 

	puts CURRENT_VERSION
	
	zip_file=File.join(packageDir, "billing_#{CURRENT_VERSION}.zip")
	
	# if exist the file delete it
	if FileTest.exists?("#{zip_file}") then
		File.delete("#{zip_file}")
	end	
	system("call #{ZIP_EXE} a -xr!*.pdb #{zip_file} #{WEBSERVER_ROOT}")	
	
	#system("call #{ZIP_EXE} a -xr!*.pdb #{zip_file} #{WIN_LOGGING_ROOT}")	
	
end

desc "Update all submodules"
task :updatemodules do
	system("call #{UPDATE_SUBMODOLUE_BAT} #{ROOT}")	
end

desc "Check your repository if there is non-utf8 files"
task :checkUtf8Files do
	ret = system("call #{CHECK_UTF8_EXE} -d:#{SOURCE_ROOT} -x:.cs,.cshtml,.aspx,.rb,.asax,.ashx,.ascx,.sql")
	raise if $?.exitstatus>0 
end

REXML::Attribute.class_eval( %q^
		def to_string
 			%Q[#@expanded_name="#{to_s().gsub(/"/, '&quot;')}"]
 		end
 	^ )

desc "Change all the projects output path, but ignore the website or web service"
task :changeOutputPath do 
	puts "change the output path of the projects"
	require 'rexml/document'
	include REXML
	Dir[ROOT+'/**/*.csproj'].each do |proj|
		puts "#{proj}"
		doc = Document.new(File.new(proj, "r"))
		changed = false
		
		# Change output path for the project
		doc.elements.each("//OutputPath") do |v|
			if v.text=="bin\\Debug\\" then
				v.text="..\\..\\bin\\Debug\\"
				changed = true
			end
			if v.text=="bin\\Release\\" then
				v.text="..\\..\\bin\\Release\\"
				changed = true
			end
			if v.text=="bin\\Live\\" then
				v.text="..\\..\\bin\\Live\\"
				changed = true
			end
		end
		
		#Treat warning as errors
		if proj.match(/Youle.Platform.Navigation/) 
			#puts "#{proj} need do the treat as warnings"
			doc.elements.each("//WarningLevel") do |v|
				el = v.next_element
				
				if el.nil?
					v.parent.elements.add(Element.new('TreatWarningsAsErrors'))
					v.next_element.text="true"
					#puts v.next_element
					
					changed=true
				end
				
			end
		end
						
		if changed==true then
			File.open(proj, 'w:utf-8'){|f| f.write doc.to_s().gsub(/\/>/, ' />')}
			# output = File.new(proj, "w")
			# doc.write output
			# output.close
		end
	end
end

desc "Change the setParameter's value to daily site"
task :modifyParameterToDaily do
	require 'rexml/document'
	include REXML
	
	infra_param_xml_path = File.join(DEPLOY_SOURCE, "BlackMamba.Billing.Website.SetParameters.xml")
	doc = Document.new(File.new(infra_param_xml_path, "r"))
	doc.elements.each("//setParameter") do |e|
		if e.attributes["name"]=="IIS Web Application Name" then
			e.attributes["value"] = "Infra_Daily"
		end
	end
	File.open(infra_param_xml_path, 'w:utf-8'){|f| f.write doc.to_s().gsub(/\/>/, ' />')}
end

task :um => :updatemodules do
	puts "update the submodules"
end

desc "Upload the dailybuild to test env"
task :uploadFTP do
	system("call #{UPLOAD_BAT} #{INFRA_DAILY}")
end

desc "warm up"
task :warmup do
	# puts "call #{WARMUP_EXE} #{WARMUP_INFRA_URL}"
	system("call #{WARMUP_EXE} #{WARMUP_INFRA_URL}")
end

task :fuscator do
	puts "fuscating..."
end

task :sign do
	puts "signing..."
	system("call s.bat #{COMPILE_MODE}")
end

#task :teamcity => [:checkUtf8Files, :addAsm, :compile, :coveragereport,:deployLocal,:compressWithMode,:AddVerToResource,:zipWeb]
task :teamcity => [:initialize,:compile, :coveragereport]

task :online => [:addAsm, :deployRemote, :deployLocal, :zipWeb]

task :onlineManual => [:addAsm, :deployLocal, :zipWeb]

# task :teamcityLive => [:addAsm, :compile, :deployLocal, :compressWithMode,:AddVerToResource,:zipWeb]

# task :teamcityDaily => [:addAsm, :compile, :deployDaily, :compressDaily,:uploadFTP, :warmup]

# task :localRun => [:checkUtf8Files, :initialize, :compile, :xunit]

task :run => [:initialize, :compile, :xunit]

task :test => []

