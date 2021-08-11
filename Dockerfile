FROM mcr.microsoft.com/dotnet/sdk:5.0

RUN apt install curl

EXPOSE 8080

WORKDIR /usr/src/app
COPY . .

RUN dotnet publish -c Release --output ./dist Oldsu.Bancho.sln 
RUN curl https://github.com/P3TERX/GeoLite.mmdb/raw/download/GeoLite2-City.mmdb

CMD ["dist/Oldsu.Bancho"]


