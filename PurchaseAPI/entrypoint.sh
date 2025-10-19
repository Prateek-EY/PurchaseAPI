#!/bin/sh

echo "Waiting for database to be ready..."
until dotnet ef database update --project ./Purchase.Infrastructure --startup-project ./PurchaseAPI; do
  echo "Database not ready yet, retrying..."
  sleep 2
done

echo "Starting app..."
dotnet PurchaseAPI.dll
