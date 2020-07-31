.PHONY = clean clean-lockfiles install relock test unit-test integration-test

clean:
	@rm -rf bin obj .nuget
	@dotnet clean

clean-lockfiles:
	@rm -rf **/**/packages.lock.json **/packages.lock.json

install:
	@dotnet restore

relock: clean-lockfiles install

test:
	@dotnet test

unit-test :
	@dotnet test --filter Category=Unit

integration-test:
	@dotnet test --filter Category=Integration