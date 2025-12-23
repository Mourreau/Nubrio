#!/usr/bin/env bash

dotnet ef migrations add "$1" \
--project Nubrio.Infrastructure \
--startup-project Nubrio.Presentation \
--output-dir Persistence/Migrations