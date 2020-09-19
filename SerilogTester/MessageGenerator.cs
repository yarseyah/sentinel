namespace SerilogTester
{
    using Bogus;

    internal class MessageGenerator
    {
        private readonly Faker<Message> faker;

        public MessageGenerator()
        {
            faker = new Faker<Message>()
                .RuleFor(m => m.Details, faker => faker.Hacker.Phrase())
                .RuleFor(m => m.Value, faker => faker.Hacker.Abbreviation());
        }

        public Message Generate() => faker.Generate();
    }
}