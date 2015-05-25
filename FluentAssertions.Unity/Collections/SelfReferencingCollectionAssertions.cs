using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

using FluentAssertions.Execution;

namespace FluentAssertions.Collections
{
    /// <summary>
    /// Contains a number of methods to assert that an <see cref="IEnumerable{T}"/> is in the expectation state.
    /// </summary>
    public class SelfReferencingCollectionAssertions<T, TAssertions> : CollectionAssertions<IEnumerable<T>, TAssertions>
        where TAssertions : SelfReferencingCollectionAssertions<T, TAssertions>
    {
        public SelfReferencingCollectionAssertions(IEnumerable<T> actualValue)
        {
            if (actualValue != null)
            {
                Subject = actualValue;
            }
        }

        /// <summary>
        /// Asserts that the number of items in the collection matches the supplied <paramref name="expected" /> amount.
        /// </summary>
        /// <param name="expected">The expected number of items in the collection.</param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion 
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="reasonArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because" />.
        /// </param>
        public AndConstraint<TAssertions> HaveCount(int expected, string because = "", params object[] reasonArgs)
        {
            if (ReferenceEquals(Subject, null))
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected {context:collection} to contain {0} item(s){reason}, but found <null>.", expected);
            }

            int actualCount = Subject.Count();

            Execute.Assertion
                .ForCondition(actualCount == expected)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected {context:collection} to contain {0} item(s){reason}, but found {1}.", expected, actualCount);

            return new AndConstraint<TAssertions>((TAssertions)this);
        }

        /// <summary>
        /// Asserts that the number of items in the collection matches a condition stated by the <paramref name="countPredicate"/>.
        /// </summary>
        /// <param name="countPredicate">A predicate that yields the number of items that is expected to be in the collection.</param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion 
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="reasonArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because" />.
        /// </param>
        public AndConstraint<TAssertions> HaveCount(Expression<Func<int, bool>> countPredicate, string because = "",
            params object[] reasonArgs)
        {
            if (countPredicate == null)
            {
                throw new NullReferenceException("Cannot compare collection count against a <null> predicate.");
            }

            if (ReferenceEquals(Subject, null))
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected {context:collection} to contain {0} items{reason}, but found {1}.", countPredicate.Body, Subject);
            }

            Func<int, bool> compiledPredicate = countPredicate.Compile();

            int actualCount = Subject.Count();

            if (!compiledPredicate(actualCount))
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected {context:collection} {0} to have a count {1}{reason}, but count is {2}.",
                        Subject, countPredicate.Body, actualCount);
            }

            return new AndConstraint<TAssertions>((TAssertions)this);
        }

        /// <summary>
        /// Expects the current collection to contain all the same elements in the same order as the collection identified by 
        /// <paramref name="elements" />. Elements are compared using their <see cref="T.Equals(T)" /> method.
        /// </summary>
        /// <param name="elements">A params array with the expected elements.</param>
        public AndConstraint<TAssertions> Equal(params T[] elements)
        {
            return Equal(elements, String.Empty);
        }

        /// <summary>
        /// Asserts that two collections contain the same items in the same order, where equality is determined using a 
        /// predicate.
        /// </summary>
        /// <param name="expectation">
        /// The collection to compare the subject with.
        /// </param>
        /// <param name="predicate">
        /// A predicate the is used to determine whether two objects should be treated as equal.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion 
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="reasonArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public AndConstraint<TAssertions> Equal(
            IEnumerable<T> expectation, Func<T, T, bool> predicate, string because = "", params object[] reasonArgs)
        {
            AssertSubjectEquality(expectation, predicate, because, reasonArgs);

            return new AndConstraint<TAssertions>((TAssertions)this);
        }

        /// <summary>
        /// Asserts that the collection contains the specified item.
        /// </summary>
        /// <param name="expected">The expectation item.</param>
        /// <param name="because">
        /// A formatted phrase explaining why the assertion should be satisfied. If the phrase does not 
        /// start with the word <i>because</i>, it is prepended to the message.
        /// </param>
        /// <param name="reasonArgs">
        /// Zero or more values to use for filling in any <see cref="string.Format(string,object[])"/> compatible placeholders.
        /// </param>
        public AndWhichConstraint<TAssertions, T> Contain(T expected, string because = "", params object[] reasonArgs)
        {
            if (ReferenceEquals(Subject, null))
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected {context:collection} to contain {0}{reason}, but found {1}.", expected, Subject);
            }

            if (!Subject.Contains(expected))
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected {context:collection} {0} to contain {1}{reason}.", Subject, expected);
            }

            return new AndWhichConstraint<TAssertions, T>((TAssertions) this,
                Subject.Where(
                    item => EqualityComparer<T>.Default.Equals(item, expected)));
        }
        
        /// <summary>
        /// Asserts that the collection contains some extra items in addition to the original items.
        /// </summary>
        /// <param name="expectedItemsList">An <see cref="IEnumerable{T}"/> of expectation items.</param>
        /// <param name="additionalExpectedItems">Additional items that are expectation to be contained by the collection.</param>
        public AndConstraint<TAssertions> Contain(IEnumerable<T> expectedItemsList,
            params T [] additionalExpectedItems)
        {
            var list = new List<T>(expectedItemsList);
            list.AddRange(additionalExpectedItems);

            return Contain((IEnumerable)list);
        }

        /// <summary>
        /// Asserts that the collection contains at least one item that matches the predicate.
        /// </summary>
        /// <param name="predicate">A predicate to match the items in the collection against.</param>
        /// <param name="because">
        /// A formatted phrase explaining why the assertion should be satisfied. If the phrase does not 
        /// start with the word <i>because</i>, it is prepended to the message.
        /// </param>
        /// <param name="reasonArgs">
        /// Zero or more values to use for filling in any <see cref="string.Format(string,object[])"/> compatible placeholders.
        /// </param>
        public AndWhichConstraint<TAssertions, T> Contain(Expression<Func<T, bool>> predicate, string because = "", params object[] reasonArgs)
        {
            if (ReferenceEquals(Subject, null))
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected {context:collection} to contain {0}{reason}, but found {1}.", predicate.Body, Subject);
            }

            Func<T, bool> func = predicate.Compile();

            Execute.Assertion
                .ForCondition(Subject.Any(func))
                .BecauseOf(because, reasonArgs)
                .FailWith("{context:Collection} {0} should have an item matching {1}{reason}.", Subject, predicate.Body);

            return new AndWhichConstraint<TAssertions, T>((TAssertions)this, Subject.Where(func));
        }

        /// <summary>
        /// Asserts that the collection only contains items that match a predicate.
        /// </summary>
        /// <param name="predicate">A predicate to match the items in the collection against.</param>
        /// <param name="because">
        /// A formatted phrase explaining why the assertion should be satisfied. If the phrase does not 
        /// start with the word <i>because</i>, it is prepended to the message.
        /// </param>
        /// <param name="reasonArgs">
        /// Zero or more values to use for filling in any <see cref="string.Format(string,object[])"/> compatible placeholders.
        /// </param>
        public AndConstraint<TAssertions> OnlyContain(
            Expression<Func<T, bool>> predicate, string because = "", params object[] reasonArgs)
        {
            Func<T, bool> compiledPredicate = predicate.Compile();

            Execute.Assertion
                .ForCondition(Subject.Any())
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected {context:collection} to contain only items matching {0}{reason}, but the collection is empty.",
                    predicate.Body);
            
            IEnumerable<T> mismatchingItems = Subject.Where(item => !compiledPredicate(item));
            if (mismatchingItems.Any())
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected {context:collection} to contain only items matching {0}{reason}, but {1} do(es) not match.",
                        predicate.Body, mismatchingItems);
            }

            return new AndConstraint<TAssertions>((TAssertions)this);
        }

        /// <summary>
        /// Asserts that the collection does not contain any items that match the predicate.
        /// </summary>
        /// <param name="predicate">A predicate to match the items in the collection against.</param>
        /// <param name="because">
        /// A formatted phrase explaining why the assertion should be satisfied. If the phrase does not 
        /// start with the word <i>because</i>, it is prepended to the message.
        /// </param>
        /// <param name="reasonArgs">
        /// Zero or more values to use for filling in any <see cref="string.Format(string,object[])"/> compatible placeholders.
        /// </param>
        public AndConstraint<TAssertions> NotContain(Expression<Func<T, bool>> predicate, string because = "", params object[] reasonArgs)
        {
            if (ReferenceEquals(Subject, null))
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected {context:collection} not to contain {0}{reason}, but found {1}.", predicate.Body, Subject);
            }

            if (Subject.Any(item => predicate.Compile()(item)))
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith("{context:Collection} {0} should not have any items matching {1}{reason}.", Subject, predicate.Body);
            }

            return new AndConstraint<TAssertions>((TAssertions)this);
        }

        /// <summary>
        /// Expects the current collection to contain only a single item.
        /// </summary>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion 
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="reasonArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because" />.
        /// </param>
        public AndWhichConstraint<TAssertions, T> ContainSingle(string because = "", params object[] reasonArgs)
        {
            if (ReferenceEquals(Subject, null))
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected {context:collection} to contain a single item but found <null>.");
            }

            if (Subject.Count() != 1)
            {
                Execute.Assertion
                       .BecauseOf(because, reasonArgs)
                       .FailWith("Expected {context:collection} to contain a single item.");
            }

            return new AndWhichConstraint<TAssertions, T>((TAssertions)this, Subject.Single());
        }

        /// <summary>
        /// Expects the current collection to contain only a single item matching the specified <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate that will be used to find the matching items.</param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion 
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="reasonArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because" />.
        /// </param>
        public AndWhichConstraint<TAssertions, T> ContainSingle(Expression<Func<T, bool>> predicate,
            string because = "", params object[] reasonArgs)
        {
            string expectationPrefix =
                string.Format("Expected {{context:collection}} to contain a single item matching {0}{{reason}}, ", predicate.Body);

            if (ReferenceEquals(Subject, null))
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith(expectationPrefix + "but found {0}.", Subject);
            }

            T[] actualItems = Subject.ToArray();
            Execute.Assertion
                .ForCondition(actualItems.Any())
                .BecauseOf(because, reasonArgs)
                .FailWith(expectationPrefix + "but the collection is empty.");

            T[] matchingElements = actualItems.Where(predicate.Compile()).ToArray();
            int count = matchingElements.Count();
            if (count == 0)
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith(expectationPrefix + "but no such item was found.");
            }
            else if (count > 1)
            {
                Execute.Assertion
                    .BecauseOf(because, reasonArgs)
                    .FailWith(expectationPrefix + "but " + count + " such items were found.");
            }
            else
            {
                // Exactly 1 item was expected
            }

            return new AndWhichConstraint<TAssertions, T>((TAssertions) this, matchingElements);
        }
    }
}