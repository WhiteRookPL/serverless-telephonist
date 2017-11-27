#!/bin/bash

IMAGE_NAME="aws-lambda-for-dotnet-with-serverless-framework"
CONTAINER_NAME="aws-lambda-telephonist-dev"

sudo docker run -w=/var/app -v `pwd`/..:/var/app -v $HOME/.aws:/home/afronski/.aws --name=$CONTAINER_NAME -it $IMAGE_NAME sh -c "bash"

sudo docker start $CONTAINER_NAME
sudo docker attach $CONTAINER_NAME
