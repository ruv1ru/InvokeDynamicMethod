using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var mth = new InvkMthd();

            var dto = mth.GetParented("CalcService", "", "ParentID", 2, "");

            Console.WriteLine($"{dto.Count} records found for ParentID: 2");
            Console.ReadLine();
        }

    }

    public class CalcService : IService<Statement>
    {

        public List<Statement> Query(Expression<Func<Statement, bool>> query)
        {

            var statements = new List<Statement>
            {
                new Statement{ ID = 1, ParentID = 2 },
                new Statement{ ID = 1, ParentID = 2 },
                new Statement{ ID = 1, ParentID = 3 }
            };

            return statements.AsQueryable().Where(query).ToList();
        }
    }

    public class InvkMthd
    {
        public MasterTableDto GetParented(string serviceName, string descriptionField, string parentField, int? parentId, string idField)
        {
            var service = ResolveService(serviceName);
            var entityType = GetMethodAllReturnType(service);

            var parentProp = entityType.GetProperty(parentField);
            var parentPropType = parentProp.GetMethod.ReturnType;



            var x = Expression.Parameter(entityType, "x");
            var parentPropExpression = Expression.PropertyOrField(x, parentField);
            var parentIdExpression = Expression.Constant(parentId);
            var parentIdSameTypeExpression = Expression.Convert(parentIdExpression, parentPropType);
            var body = Expression.Equal(parentPropExpression, parentIdSameTypeExpression);
            var lambda = Expression.Lambda(body, x);

            var parameters = new object[] { lambda };

            try
            {
                var foo = CallMethod(service, "Query", parameters);

                var statements = (List<Statement>)foo;

                var dto = new MasterTableDto { Count = statements.Count };
                return dto;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Type GetMethodAllReturnType(object service)
        {
            return new Statement().GetType();
        }

        private object ResolveService(string serviceName)
        {
            return new CalcService();
        }

        public object CallMethod(object service, string methodName, object[] parameters)
        {
            var serviceType = service.GetType();

            var method = serviceType.GetMethods().Where(mf => mf.GetParameters().Count() == 1).First();
            var result = method.Invoke(service, parameters); // Error :'(

            return result;
        }
    }

    public interface IService<TEntity> where TEntity : class
    {
        List<TEntity> Query(Expression<Func<TEntity, bool>> query);
    }


    public class Statement
    {
        public int ID { get; set; }
        public int? ParentID { get; set; }
    }



}
