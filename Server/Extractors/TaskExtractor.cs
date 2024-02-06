using Server.Interfaces;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Server.Extractors
{
    public class TaskExtractor : IExtractor<Task>
    {
        private readonly ConcurrentDictionary<Type, Func<Task, object>> _extractors = new();

        /// <summary>
        /// Extracts the result value from a completed Task.
        /// </summary>
        /// <param name="input">The Task from which to extract the result.</param>
        /// <returns>The result of the completed Task as an object.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the input Task is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the Task is not completed or if it's not a generic Task</exception>
        public object? ExtractValue(Task input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (!input.IsCompleted)
            {
                throw new InvalidOperationException("Task is not completed.");
            }

            Type taskType = input.GetType();
            if (!taskType.IsGenericType || taskType.GetGenericTypeDefinition() != typeof(Task<>))
            {
                throw new InvalidOperationException("Invalid task type. Only generic Task<> types are supported.");
            }

            var extractor = _extractors.GetOrAdd(taskType, CreateExtractor);

            return extractor(input);
        }

        /// <summary>
        /// Creates a function to extract the result from a Task of a specific type.
        /// </summary>
        /// <param name="taskType">The type of the Task to create an extractor for.</param>
        /// <returns>A function that takes a Task and returns its result as an object</returns>
        /// <exception cref="InvalidOperationException">Thrown if the provided type is not a Task or is not assignable from Task.</exception>
        private Func<Task, object> CreateExtractor(Type taskType)
        {
            if (!typeof(Task).IsAssignableFrom(taskType))
            {
                throw new InvalidOperationException("Invalid task type.");
            }
            ParameterExpression param = Expression.Parameter(typeof(Task), "task");
            UnaryExpression taskCast = Expression.Convert(param, taskType);
            MemberExpression propertyAccess = Expression.Property(taskCast, "Result");
            UnaryExpression convertToObj = Expression.Convert(propertyAccess, typeof(object));

            return Expression.Lambda<Func<Task, object>>(convertToObj, param).Compile();
        }
    }
}
