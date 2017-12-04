#!/bin/bash

OPERATION=${1}
PROFILE=${2:-"private"}

STACK_NAME="serverless-telephonist-infrastructure"

if [[ "${OPERATION}" == "create" ]]; then

  aws cloudformation create-stack                           \
    --profile ${PROFILE}                                    \
    --stack-name "${STACK_NAME}"                            \
    --template-body "file://$(pwd)/${STACK_NAME}.yaml"      \
    --capabilities "CAPABILITY_IAM" "CAPABILITY_NAMED_IAM"

elif [[ "${OPERATION}" == "delete" ]]; then

  aws cloudformation delete-stack                           \
    --profile ${PROFILE}                                    \
    --stack-name "${STACK_NAME}"

else
  echo "Unknown operation passed as a first argument (only 'create' or 'delete' are supported)."
  exit 1
fi

exit 0
