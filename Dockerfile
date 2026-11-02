# Sử dụng image SDK của .NET để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy file project và restore các gói NuGet
COPY *.csproj ./
RUN dotnet restore

# Copy toàn bộ source code và build
COPY . .
RUN dotnet publish -c Release -o out

# Sử dụng image runtime nhẹ hơn để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Quan trọng: Render sẽ tự inject biến PORT, không hardcode port ở đây
# Lệnh khởi chạy ứng dụng, thay "TenDuAn.dll" bằng tên file .dll thực tế của bạn
ENTRYPOINT ["dotnet", "RescuePets.dll"]