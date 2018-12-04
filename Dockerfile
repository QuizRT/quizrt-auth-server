FROM microsoft/dotnet:sdk AS build-env

COPY . /authapp

WORKDIR /authapp

RUN ["dotnet", "restore"]

RUN ["dotnet", "build"]

RUN chmod +x ./entrypoint.sh

CMD /bin/bash ./entrypoint.sh