using System;
using System.Collections.Generic;
using System.Text;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.MessagePack;
using MessagePack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Leo.UnitTest
{
    [TestClass]
    public class MessagePackTest
    {
        [TestMethod]
        public void TestCodec()
        {
            Person person = new Person
            {
                Name = "张宏伟",
                Age = 18
            };
            TransportMessage transportMessage = new TransportMessage
            {
                Id = "1",
                ContentType = "Person",
                Content = person
            };
            MessagePackTransportMessageCodecFactory factory = new MessagePackTransportMessageCodecFactory();
            ITransportMessageEncoder encoder = factory.GetEncoder();
            ITransportMessageDecoder decoder = factory.GetDecoder();
            byte[] vs = encoder.Encode(transportMessage);
            TransportMessage message =decoder.Decode(vs);
            Assert.AreEqual(message.Id, "1");
            Assert.AreEqual(message.ContentType, "Person");
            Assert.AreEqual(((object[])message.Content)[0].ToString(), "张宏伟" );
            Assert.AreEqual(((object[])message.Content)[1].ToString(), "18");
        }

        [MessagePackObject]
        public class Person
        {
            [Key(0)]
            public string Name { get; set; }
            [Key(1)]
            public int Age { get; set; }
        }
    }
}
