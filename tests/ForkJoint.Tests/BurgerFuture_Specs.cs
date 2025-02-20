namespace ForkJoint.Tests
{
    using System.Threading.Tasks;
    using Api.Components.Activities;
    using Api.Components.Futures;
    using Api.Components.ItineraryPlanners;
    using Api.Services;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;


    [TestFixture]
    public class BurgerFuture_Specs :
        FutureTestFixture
    {
        [Test]
        public async Task Should_complete()
        {
            var orderId = NewId.NextGuid();
            var orderLineId = NewId.NextGuid();

            IRequestClient<OrderBurger> client = TestHarness.GetRequestClient<OrderBurger>();

            Response<BurgerCompleted> response = await client.GetResponse<BurgerCompleted>(new
            {
                OrderId = orderId,
                OrderLineId = orderLineId,
                Burger = new Burger
                {
                    BurgerId = orderLineId,
                    Weight = 1.0m,
                    Cheese = true,
                }
            });

            Assert.That(response.Message.OrderId, Is.EqualTo(orderId));
            Assert.That(response.Message.OrderLineId, Is.EqualTo(orderLineId));
            Assert.That(response.Message.Burger.Cheese, Is.True);
            Assert.That(response.Message.Burger.Weight, Is.EqualTo(1.0m));
        }

        [Test]
        public async Task Should_fault()
        {
            var orderId = NewId.NextGuid();
            var orderLineId = NewId.NextGuid();

            IRequestClient<OrderBurger> client = TestHarness.GetRequestClient<OrderBurger>();

            try
            {
                await client.GetResponse<BurgerCompleted>(new
                {
                    OrderId = orderId,
                    OrderLineId = orderLineId,
                    Burger = new Burger
                    {
                        BurgerId = orderLineId,
                        Weight = 1.0m,
                        Cheese = true,
                        Lettuce = true
                    }
                });

                Assert.Fail("Should have thrown");
            }
            catch (RequestFaultException exception)
            {
                Assert.That(exception.Fault.Host, Is.Not.Null);
                Assert.That(exception.Message, Contains.Substring("lettuce"));
            }
        }

        protected override void ConfigureServices(IServiceCollection collection)
        {
            collection.AddSingleton<IGrill, Grill>();
            collection.AddScoped<IItineraryPlanner<OrderBurger>, BurgerItineraryPlanner>();
        }

        protected override void ConfigureMassTransit(IBusRegistrationConfigurator configurator)
        {
            configurator.AddActivitiesFromNamespaceContaining<GrillBurgerActivity>();

            configurator.AddFuture<BurgerFuture>();
            configurator.AddFuture<OnionRingsFuture>();
        }
    }
}