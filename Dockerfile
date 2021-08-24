#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get install -y --fix-missing  \
        wget \
        unzip \
        libgtk-3-0 \
        xvfb \
        libxss1 \
        libnss3 \
        libasound2 \
        libgdiplus
        ENV DISPLAY :9.0

RUN wget -O /fuck-xuexiqiangguo.zip https://github.com/fuck-xuexiqiangguo/Fuck-XueXiQiangGuo/raw/master/Fuck学习强国-linux.zip && \
    unzip -q -d /app/ fuck-xuexiqiangguo.zip && \
    rm /fuck-xuexiqiangguo.zip && \
    chmod +x /app/Fuck学习强国

ENV TZ=Asia/Shanghai
ENV Serverid=
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
WORKDIR /app/

COPY init.sh /
RUN chmod +x /script.sh
COPY ["xuexihelper/xuexihelper.csproj", "xuexihelper/"]
RUN dotnet restore "xuexihelper/xuexihelper.csproj"
COPY . .
WORKDIR "/src/xuexihelper"
RUN dotnet build "xuexihelper.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "xuexihelper.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "xuexihelper.dll"]