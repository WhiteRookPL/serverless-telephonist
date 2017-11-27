#!/bin/bash

IMAGE_NAME="aws-lambda-for-dotnet-with-serverless-framework"
CURRENT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

sudo docker build --rm -t $IMAGE_NAME -f $CURRENT_DIR/Dockerfile $CURRENT_DIR
