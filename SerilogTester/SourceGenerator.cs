namespace SerilogTester
{
    using Bogus;

    internal class SourceGenerator
    {
        private readonly Faker<Source> faker;

        public SourceGenerator()
        {
            faker = new Faker<Source>().RuleFor(s => s.Name, faker => faker.Hacker.Phrase()).RuleFor(
                s => s.Status,
                faker => faker.Commerce.ProductAdjective());
        }

        public Source Generate() => faker.Generate();
    }
}