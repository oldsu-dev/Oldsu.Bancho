FROM mcr.microsoft.com/dotnet/sdk:5.0

RUN apt install curl

EXPOSE 8080

WORKDIR /usr/src/app
COPY . .

RUN dotnet publish -c Release --output ./dist Oldsu.Bancho.sln 

CMD ["dist/Oldsu.Bancho"]


