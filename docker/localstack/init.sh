#!/bin/bash
# Crea el bucket de S3 y las colas de SQS cuando LocalStack está listo.
set -e

awslocal s3 mb "s3://$S3_BUCKET"

# Cola de fallidos: recibe los mensajes que fallaron 3 veces.
awslocal sqs create-queue --queue-name "$SQS_COLA_FALLIDOS"
DLQ_URL=$(awslocal sqs get-queue-url --queue-name "$SQS_COLA_FALLIDOS" --query QueueUrl --output text)
DLQ_ARN=$(awslocal sqs get-queue-attributes --queue-url "$DLQ_URL" --attribute-names QueueArn --query Attributes.QueueArn --output text)

# Cola principal: reserva de 5 minutos (el worker la renueva) y redirección a fallidos.
awslocal sqs create-queue --queue-name "$SQS_COLA" --attributes "{
  \"VisibilityTimeout\": \"300\",
  \"RedrivePolicy\": \"{\\\"deadLetterTargetArn\\\":\\\"$DLQ_ARN\\\",\\\"maxReceiveCount\\\":\\\"3\\\"}\"
}"
