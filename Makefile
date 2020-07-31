.PHONY = clean install test unit-test integration-test
configuration = Debug
target_framework = netcoreapp3.1
output_path = bin/Project.Name/$(configuration)/$(target_framework)

clean:
	@rm -rf bin obj .nuget
	@dotnet clean

clean-lockfiles:
	@rm -rf **/**/packages.lock.json **/packages.lock.json

install:
	@dotnet restore

test:
	@dotnet test

unit-test :
	@dotnet test --filter Category=Unit

integration-test:
	@dotnet test --filter Category=Integration

$(output_path)/Project.Name.dll:
	@dotnet build