using System;
using System.Linq;
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace test_sqs
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            IAmazonSQS sqs = new AmazonSQSClient(RegionEndpoint.APSoutheast2);

            var sqsRequest = new CreateQueueRequest("martin-test.fifo");
            sqsRequest.Attributes.Add("FifoQueue", "true");
            sqsRequest.Attributes.Add("ContentBasedDeduplication", "true");

            var creteQueueResponse = sqs.CreateQueueAsync(sqsRequest).Result;
            var myQueueUrl = creteQueueResponse.QueueUrl;
            Console.WriteLine($"Created QueueUrl: {myQueueUrl}");

            //var listQueuesRequest = new ListQueuesRequest();
            //var listQueuesResponse = sqs.ListQueuesAsync(listQueuesRequest);
            //foreach(var queueUrl in listQueuesResponse.Result.QueueUrls)
            //{
            //    Console.WriteLine($"QueueUrl: {queueUrl}");
            //}

            for (var i = 0; i < 3; i++)
            {
                var sqsMesageRequest = new SendMessageRequest
                {
                    QueueUrl = myQueueUrl,
                    MessageBody = "hello world man!" + DateTime.Today.ToLongTimeString(),
                    MessageGroupId = "group1"
                };
                SendMessageResponse messageResult = sqs.SendMessageAsync(sqsMesageRequest).Result;
                Console.WriteLine(messageResult.ToString());
                Console.WriteLine("Message send to our sqs queue");
            }



            var receiveMessageRequest = new ReceiveMessageRequest
            {
               QueueUrl = myQueueUrl,
               MaxNumberOfMessages = 10,
               VisibilityTimeout = 30
            };

            System.Threading.Thread.Sleep(10000);
            ReceiveMessageResponse receiveMessageResponse = sqs.ReceiveMessageAsync(receiveMessageRequest).Result;



            foreach (var message in receiveMessageResponse.Messages)
            {
                Console.WriteLine("Message\n");
                Console.WriteLine($" MessageId: {message.MessageId}");
                Console.WriteLine($" ReceiptHandle: {message.ReceiptHandle}");
                Console.WriteLine($" Body: {message.Body}");

                
                foreach(var attribute in message.Attributes)
                {
                    Console.WriteLine("Attribute\n");
                    Console.WriteLine($"  Name: {attribute.Key}");
                    Console.WriteLine($"  Value: {attribute.Value}");
                }


                var messageReceiptHandle = receiveMessageResponse.Messages.FirstOrDefault()?.ReceiptHandle;

                var deleteRequest = new DeleteMessageRequest(myQueueUrl, messageReceiptHandle);
                sqs.DeleteMessageAsync(deleteRequest);
            }


            Console.Read();



        }
    }
}
