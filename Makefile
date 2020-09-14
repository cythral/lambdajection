.PHONY = clean clean-lockfiles install relock test unit-test integration-test

clean:
	@rm -rf bin obj .nuget examples/.nuget
	@dotnet clean

clean-lockfiles:
	@for file in $$(find . -name packages.lock.json); do rm $$file; done

install:
	@dotnet restore

relock: clean-lockfiles install

test:
	@dotnet test

unit-test :
	@dotnet test --filter Category=Unit

integration-test:
	@dotnet test --filter Category=Integration