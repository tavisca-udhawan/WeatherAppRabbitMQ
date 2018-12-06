using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tavisca.Platform.Common.Serialization;
using Tavisca.Platform.Common.Plugins.Json;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{
    public class SerializationFixture
    {
        [Fact]
        public void SimpleSerializationTest()
        {
            var person = new Person
            {
                Name = "John Doe",
                Age = 25,
                Hobbies = { "Reading", "Travelling" }
            };
            var serializer = new JsonDotNetSerializer(new TestTranslatorMapping());
            string json;
            using (var stream = new MemoryStream())
            {
                serializer.Serialize<Person>(stream, person);
                json = Encoding.UTF8.GetString(stream.ToArray());
            }
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var copy = serializer.Deserialize<Person>(stream);
                Assert.True(person.IsSameAs(copy));
            }
            

        }
    }

    public class TestTranslatorMapping : ITranslatorMapping
    {
        public void Configure(IDictionary<Type, JsonConverter> mapping)
        {
            mapping[typeof(Person)] = new PersonTranslator();
        }
    }

    [Serializable]
    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public List<string> Hobbies { get; } = new List<string>();

        public bool IsSameAs(Person other)
        {
            if (other == null)
                return false;
            return
                Name.Equals(other.Name) &&
                Age.Equals(other.Age) &&
                AreListEqual(Hobbies, other.Hobbies) == true;
        }

        private bool AreListEqual(List<string> listA, List<string> listB)
        {
            if (listA.Count != listB.Count)
                return false;
            return listA.Except(listB, StringComparer.OrdinalIgnoreCase).Any() == false;
        }
    }

    public class PersonTranslator : JsonTranslator<Person>
    {
        protected override Person CreateNew()
        {
            return new Person();
        }

        protected override void Serialize(JsonWriter writer, Person value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WriteField("name", value.Name);
            writer.WriteField("age", value.Age);
            writer.WriteField("hobbies", value.Hobbies, serializer);
            writer.WriteEndObject();
        }

        protected override void SetupParser(IParserSetup<Person> parser)
        {
            parser
                .Setup("name", (p, v) => p.Name = v.AsString())
                .Setup("age", (p, v) => p.Age = v.AsInt(13))
                .Setup("hobbies", (p, v) => p.Hobbies.AddRange(v.AsArray<string>()));
        }
    }
}
