FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 前端构建阶段（使用 Bun 获得更快的构建速度）
FROM oven/bun:1-alpine AS frontend-build
WORKDIR /web
COPY web/package.json web/bun.lock* ./
# 使用 bun install --frozen-lockfile 确保依赖版本一致
RUN bun install --frozen-lockfile
COPY web/ ./
RUN bun run build

# 后端构建阶段
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/OneAI/OneAI.csproj", "src/OneAI/"]
RUN dotnet restore "src/OneAI/OneAI.csproj"
COPY . .
WORKDIR "/src/src/OneAI"
RUN dotnet build "./OneAI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 发布阶段（包含前端静态文件）
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OneAI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 将前端构建产物复制到 wwwroot 目录
COPY --from=frontend-build /web/dist /app/publish/wwwroot

# 最终阶段
FROM base AS final
USER root
# 设置时区为上海时间
RUN apt-get update && apt-get install -y tzdata \
    && ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime \
    && echo "Asia/Shanghai" > /etc/timezone \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OneAI.dll"]
