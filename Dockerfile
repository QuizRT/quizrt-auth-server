FROM microsoft/dotnet:sdk AS build-env

COPY . /socialapp

WORKDIR /socialapp

RUN ["dotnet", "restore"]

RUN ["dotnet", "build"]

RUN chmod +x ./entrypoint.sh

CMD /bin/bash ./entrypoint.sh