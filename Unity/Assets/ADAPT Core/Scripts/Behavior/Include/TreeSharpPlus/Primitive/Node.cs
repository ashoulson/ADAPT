#region License
/*
* A simplistic Behavior Tree implementation in C#
* 
* Copyright (C) 2011-2015 Alexander Shoulson - ashoulson@gmail.com
* (TreeSharp Copyright (C) 2010-2011 ApocDev apocdev@gmail.com)
* 
* This file is part of TreeSharpPlus.
* 
* TreeSharpPlus is free software: you can redistribute it and/or modify
* it under the terms of the GNU Lesser General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* TreeSharpPlus is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Lesser General Public License for more details.
* 
* You should have received a copy of the GNU Lesser General Public License
* along with TreeSharpPlus.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

// Define me for better debug output
//#define VERBOSE

using System;
using System.Collections.Generic;

using UnityEngine;

namespace TreeSharpPlus
{
    /// <summary>
    /// The base class of the entire behavior tree system.
    /// All branches derive from this class.
    /// </summary>
    public abstract class Node : IEquatable<Node>
    {
        // TODO: Revisit locking later down the line
        // protected static readonly object Locker = new object();

#if VERBOSE
        // Store the stack trace when we were created
        private string stackTrace;
#endif

        private IEnumerator<RunStatus> _current;

        protected Node()
        {
#if VERBOSE
            this.stackTrace = System.Environment.StackTrace;
#endif
            this.IsRunning = false;
            this.IsTerminating = false;
            Guid = Guid.NewGuid();
            CleanupHandlers = new Stack<CleanupHandler>();
        }

        public bool IsRunning { get; protected set; }
        public bool IsTerminating { get; protected set; }
        public RunStatus? LastStatus { get; protected set; }
        public RunStatus? LastTerminationStatus { get; protected set; }

        protected Stack<CleanupHandler> CleanupHandlers { get; set; }

        public Node Parent { get; set; }

        /// <summary>
        /// Simply an identifier to make sure each composite is 'unique'.
        /// Useful for XML declaration parsing.
        /// </summary>
        public Guid Guid { get; protected set; }

        #region IEquatable<Node> Members
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name = "other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name = "other">An object to compare with this object.</param>
        public bool Equals(Node other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return other.Guid.Equals(Guid);
        }
        #endregion

        /// <summary>
        /// Determines whether the specified <see cref = "T:System.Object" /> is equal to the current <see cref = "T:System.Object" />.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref = "T:System.Object" /> is equal to the current <see cref = "T:System.Object" />; otherwise, false.
        /// </returns>
        /// <param name = "obj">The <see cref = "T:System.Object" /> to compare with the current <see cref = "T:System.Object" />. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != typeof(Node))
            {
                return false;
            }
            return Equals((Node)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref = "T:System.Object" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        public static bool operator ==(Node left, Node right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Node left, Node right)
        {
            return !Equals(left, right);
        }

        public abstract IEnumerable<RunStatus> Execute();

        public RunStatus Tick()
        {
#if VERBOSE
            if (LastStatus.HasValue && LastStatus != RunStatus.Running)
                throw new ApplicationException(
                    this 
                    + ".Tick(): Tick on non-running node \nTrace:\n" 
                    + this.stackTrace);
            if (this.IsTerminating == true)
                throw new ApplicationException(
                    this 
                    + ".Tick(): Tick on terminating node \nTrace:\n" 
                    + this.stackTrace);
            if (_current == null)
                throw new ApplicationException(
                    this 
                    + ".Tick(): Tick on uninitialized node \nTrace:\n" 
                    + this.stackTrace);

            if (_current.MoveNext())
                LastStatus = _current.Current;
            else
                throw new ApplicationException(
                    this 
                    + ".Tick(): Unexpected iterator termination \nTrace:\n" 
                    + this.stackTrace
                    + "\n\n"
                    + PrintTree(this));
#else
            if (LastStatus.HasValue && LastStatus != RunStatus.Running)
                throw new ApplicationException(
                    this 
                    + ".Tick(): Tick on non-running node");
            if (this.IsTerminating == true)
                throw new ApplicationException(
                    this 
                    + ".Tick(): Tick on terminating node");
            if (_current == null)
                throw new ApplicationException(
                    this 
                    + ".Tick(): Tick on uninitialized node");

            if (_current.MoveNext())
                LastStatus = _current.Current;
            else
                throw new ApplicationException(
                    this 
                    + ".Tick(): Unexpected iterator termination");
#endif

            RunStatus result = this.LastStatus.Value;
            if (result != RunStatus.Running)
                this.Stop();

            return result;
        }

        public virtual void Start()
        {
            this.IsRunning = true;
            this.LastStatus = null;
            this.IsTerminating = false;
            this.LastTerminationStatus = null;
            this._current = this.Execute().GetEnumerator();
        }

        public virtual void Stop()
        {
            this.IsRunning = false;
            this.Cleanup();
            if (this._current != null)
            {
                this._current.Dispose();
                this._current = null;
            }
        }

        /// <summary>
        /// Propagated through a tree or subtree that is stopped or interrupted
        /// while executing. Can be called at any time. Used for cleanup or to
        /// return an agent to a neutral state. Handles stopping the node as
        /// well. Note that this function may be called multiple times even 
        /// after reporting success or failure. Classes implementing this
        /// function are encouraged to use StartTermination() and
        /// ReturnTermination, as seen here.
        /// </summary>
        public virtual RunStatus Terminate()
        {
            // Nothing to do here, just start and succeed.
            RunStatus curStatus = this.StartTermination();
            if (curStatus != RunStatus.Running)
                return curStatus;
            return this.ReturnTermination(RunStatus.Success);
        }

        /// <summary>
        /// Call this at the beginning of every termination function to set things
        /// up. If it returns Success or Failure, you should return that immediately.
        /// 
        /// Example:
        ///    RunStatus curStatus = this.StartTermination();
        ///    if (curStatus != RunStatus.Running)
        ///        return curStatus;
        /// </summary>
        protected virtual RunStatus StartTermination()
        {
            if (this.IsRunning == false)
            {
                // We aren't running. That means we either weren't
                // running when the termination process began, or
                // we've already terminated

                // We weren't running to begin with
                if (this.LastTerminationStatus == null)
                {
                    // That's fine, just return success since
                    // we have nothing to do
                    return RunStatus.Success;
                }
                else
                {
                    // This node has been terminated
                    if (this.LastTerminationStatus == RunStatus.Running)
                    {
                        // This is really weird and shouldn't happen.
                        // The node has already stopped, but we still
                        // think we're terminating. If you're getting
                        // this error, you may be calling Stop() in
                        // the termination function. Don't do that.
                        // Use ReturnTermination() properly instead.
                        throw new ApplicationException(
                            "StartTermination(): Bad RunStatus");
                    }
                    else
                    {
                        // We already terminated successfully or failed, 
                        // so return whichever result it was
                        return (RunStatus)this.LastTerminationStatus;
                    }
                }
            }
            // We have been running. Have we started terminating yet?
            // If not, start the process.
            else if (this.LastTerminationStatus == null)
            {
                this.IsTerminating = true;
                this.LastTerminationStatus = RunStatus.Running;
            }

            // We're in the process of terminating, so return whatever
            // we left off with
            return (RunStatus)this.LastTerminationStatus;
        }

        /// <summary>
        /// Call this whenever you're going to return a RunStatus from a
        /// Terminate function. It will error-check the result and clean up
        /// if necessary, and return whatever it was given (if it's valid).
        /// Also handles storage of the last termination status.
        /// </summary>
        protected virtual RunStatus ReturnTermination(RunStatus result)
        {
            // Do some error checking. We can only go from 
            // {Running} to {Running, Success, Failure}
            // TODO: Wrap this in an ifdef so it can be disabled for speed - AS
            if (this.LastTerminationStatus == null)
                // We somehow never started terminating, this is really weird
                throw new ApplicationException(
                    this + ".ReturnTermination(): Bad RunStatus (1)");

            if (this.LastTerminationStatus != RunStatus.Running
                && result != this.LastTerminationStatus)
                // We were already at Success or Failure, and got something different
                throw new ApplicationException(
                    this + "ReturnTermination(): Bad RunStatus (2)");

            // We got a valid ending result, either Success or Failure
            if (result != RunStatus.Running)
            {
                this.Stop();
                this.IsTerminating = false;
            }

            this.LastTerminationStatus = result;
            return result;
        }

        /// <summary>
        /// Clears the LastStatus field. Useful for debugging and tracing
        /// the source of a success or failure when a tree terminates.
        /// </summary>
        public virtual void ClearLastStatus()
        {
            this.LastStatus = null;
        }

        protected void Cleanup()
        {
            if (CleanupHandlers.Count != 0)
            {
                while (CleanupHandlers.Count != 0)
                {
                    CleanupHandlers.Pop().Dispose();
                }
            }
        }

        public static string PrintTree(Node node, Func<Node, string> func = null)
        {
            return Node.PrintTree(node, 0, func);
        }

        protected static string PrintTree(Node node, int indent, Func<Node, string> func = null)
        {
            string str = "";
            for (int i = 0; i < indent; i++)
                str += "   ";
            str += node.GetType() + "[" + node.GetHashCode() + "]";

            if (func != null)
                str += " " + func(node);

            if (node is NodeGroup)
            {
                foreach (Node n in ((NodeGroup)node).Children)
                    str += "\n" + PrintTree(n, indent + 1, func);
            }
            return str;
        }

        #region Nested type: CleanupHandler

        protected abstract class CleanupHandler : IDisposable
        {
            protected CleanupHandler(Node owner)
            {
                this.Owner = owner;
            }

            protected Node Owner { get; set; }

            private bool IsDisposed { get; set; }

            #region IDisposable Members

            public void Dispose()
            {
                if (!IsDisposed)
                {
                    this.IsDisposed = true;
                    this.DoCleanup();
                }
            }

            #endregion

            protected abstract void DoCleanup();
        }

        #endregion
    }
}