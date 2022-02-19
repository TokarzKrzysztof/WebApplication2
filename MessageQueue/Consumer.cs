using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JMSProxyLib;
using System.IO;
using System.Threading;
using System.IO.Ports;
using System.Net.Sockets;

namespace JMSClient
{
	public class Consumer
	{
		static String BROKER_HOST = "147.135.208.219";//"hou-12e74d9341";
		static int BROKER_PORT = 7676;
		static String QUEUE = "Test";
		static String BROKER_USERID = "admin";
		static String BROKER_PASSWORD = "admin";

		public bool MessageHandler(String msg)
		{
			Console.WriteLine("[C# Received]: " + msg);

			//StreamWriter wtr = new StreamWriter(BASE_PATH + "Msg_Num_" + (cnt++) + ".xml");
			//wtr.Write(msg);
			//wtr.Flush();
			//wtr.Close();

			return !msg.Equals("END");
		}

		static String BASE_PATH = @".\";
		static int cnt = 1;
		public bool ProcessMessageDelegateAsync(String msg)
		{
			Console.WriteLine("[C# Received]: " + msg.Substring(0,75));

			StreamWriter wtr = new StreamWriter(BASE_PATH+"Msg_Num_"+(cnt++)+".xml");
			wtr.Write(msg);
			wtr.Flush();
			wtr.Close();

			return !msg.Equals("END");
		}

		public static void Start()
		{
			try
			{
				Consumer p = new Consumer();
				bool doAsync = false;

				JMSProxy jms = new JMSProxy();			
				jms.CreateQueueConnection(BROKER_HOST, BROKER_PORT, QUEUE,
									 BROKER_USERID, BROKER_PASSWORD, doAsync);

                PrepareGetMessages(doAsync, jms, p);
                //PrepareSendMessages(jms);


                jms.DestroyTopicConsumer();

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				Console.Read();
			}
		}

        public static void PrepareSendMessages(JMSProxy jms)
        {
			jms.CreateQueueProducer();
			jms.SendMessage("test sent message");
        }

        public static void PrepareGetMessages(bool doAsync, JMSProxy jms, Consumer p)
        {
			if (doAsync)
			{
				Console.WriteLine("Starting ASYNC Consumer");
				jms.CreateQueueConsumerAsync(p.ProcessMessageDelegateAsync);
				bool stop = false;
				Console.WriteLine("==>Enter 'q' to quit");
				do
				{
					stop = Console.Read() == 'q';
				} while (!stop);
			}
			else
			{
				Console.WriteLine("Starting SYNC Consumer");
				jms.CreateQueueConsumerSync();
				Console.WriteLine("==>Send (from producer) 'END' to quit");
				// this is a blocking call so it won't return until we are ready to shutdown
				jms.ConsumeQueueMessages(p.MessageHandler);
			}
		}
	}
}
