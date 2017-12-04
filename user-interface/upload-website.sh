#!/bin/bash

BUCKET="${1:-serverless-telephonist-ui}"
PROFILE="${2:-private}"

aws s3  sync . "s3://${BUCKET}" --exclude "*.sh" --profile="${PROFILE}" --acl="public-read"
