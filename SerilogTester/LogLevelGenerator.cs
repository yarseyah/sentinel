namespace SerilogTester
{
    using Bogus;

    internal class LogLevelGenerator
    {
        private readonly Faker faker;

        public LogLevelGenerator()
        {
            faker = new Faker();
        }

        public int Generate() => faker.Random.Int(0, 8);
    }
}