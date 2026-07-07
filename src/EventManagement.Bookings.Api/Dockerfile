# Берём официальный образ .NET SDK
# Он нужен ТОЛЬКО для сборки проекта (в нём есть компилятор)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Устанавливаем рабочую директорию внутри контейнера
# (это как папка проекта внутри Docker)
WORKDIR /src

# Копируем ВСЕ файлы проекта в контейнер
# Точка "." означает текущую папку (где Dockerfile лежит)
COPY . .

# Восстановление зависимостей .NET-проекта.
RUN dotnet restore \
    src/EventManagement.Bookings.Api/EventManagement.Bookings.Api.csproj

# Собираем проект в Release-режиме
# -c Release → оптимизированная сборка
# -o /app → куда сохранить результат публикации
RUN dotnet publish \
    src/EventManagement.Bookings.Api/EventManagement.Bookings.Api.csproj \
    -c Release \
    -o /app/publish

# Берём лёгкий образ .NET Runtime (без SDK!)
# Он нужен ТОЛЬКО для запуска уже собранного приложения
FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Рабочая папка внутри контейнера
WORKDIR /app

# Копируем результат сборки из предыдущего этапа (build)
# из /app (build stage) → в /app (runtime stage)
COPY --from=build /app/publish .

# Принимает соединения на порту 8080 внутри контейнера
EXPOSE 8080

# Команда запуска контейнера
ENTRYPOINT ["dotnet","EventManagement.Bookings.Api.dll"]