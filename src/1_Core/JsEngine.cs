﻿//MIT, 2015-2017, WinterDev, EngineKit, brezza92
//MIT, 2013, Federico Di Gregorio <fog@initd.org>

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Espresso
{
    public partial class JsEngine : IDisposable
    {

        readonly KeepaliveRemoveDelegate _keepalive_remove;
        readonly KeepAliveGetPropertyValueDelegate _keepalive_get_property_value;
        readonly KeepAliveSetPropertyValueDelegate _keepalive_set_property_value;
        readonly KeepAliveValueOfDelegate _keepalive_valueof;
        readonly KeepAliveInvokeDelegate _keepalive_invoke;
        readonly KeepAliveDeletePropertyDelegate _keepalive_delete_property;
        readonly KeepAliveEnumeratePropertiesDelegate _keepalive_enumerate_properties;

        readonly Dictionary<int, JsContext> _aliveContexts = new Dictionary<int, JsContext>();
        readonly Dictionary<int, JsScript> _aliveScripts = new Dictionary<int, JsScript>();

        int _currentContextId = 0;
        int _currentScriptId = 0;
        readonly HandleRef _engine;//native js engine 
        JsTypeDefinitionBuilder defaultTypeBuilder;



        public JsEngine(JsTypeDefinitionBuilder defaultTypeBuilder, int maxYoungSpace, int maxOldSpace)
        {

            _keepalive_remove = new KeepaliveRemoveDelegate(KeepAliveRemove);
            _keepalive_get_property_value = new KeepAliveGetPropertyValueDelegate(KeepAliveGetPropertyValue);
            _keepalive_set_property_value = new KeepAliveSetPropertyValueDelegate(KeepAliveSetPropertyValue);
            _keepalive_valueof = new KeepAliveValueOfDelegate(KeepAliveValueOf);
            _keepalive_invoke = new KeepAliveInvokeDelegate(KeepAliveInvoke);
            _keepalive_delete_property = new KeepAliveDeletePropertyDelegate(KeepAliveDeleteProperty);
            _keepalive_enumerate_properties = new KeepAliveEnumeratePropertiesDelegate(KeepAliveEnumerateProperties);

            _engine = new HandleRef(this, jsengine_new(
                _keepalive_remove,
                _keepalive_get_property_value,
                _keepalive_set_property_value,
                _keepalive_valueof,
                _keepalive_invoke,
                _keepalive_delete_property,
                _keepalive_enumerate_properties,
                maxYoungSpace,
                maxOldSpace));
            this.defaultTypeBuilder = defaultTypeBuilder;
        }
        public JsEngine(IntPtr nativeJsEnginePtr)
            : this(nativeJsEnginePtr, new DefaultJsTypeDefinitionBuilder())
        {
        }
        public JsEngine(IntPtr nativeJsEnginePtr,
            JsTypeDefinitionBuilder defaultTypeBuilder)
        {
            //native js engine is created from native side
            //for this managed object
            //so we add more managed function to handle
            _keepalive_remove = new KeepaliveRemoveDelegate(KeepAliveRemove);
            _keepalive_get_property_value = new KeepAliveGetPropertyValueDelegate(KeepAliveGetPropertyValue);
            _keepalive_set_property_value = new KeepAliveSetPropertyValueDelegate(KeepAliveSetPropertyValue);
            _keepalive_valueof = new KeepAliveValueOfDelegate(KeepAliveValueOf);
            _keepalive_invoke = new KeepAliveInvokeDelegate(KeepAliveInvoke);
            _keepalive_delete_property = new KeepAliveDeletePropertyDelegate(KeepAliveDeleteProperty);
            _keepalive_enumerate_properties = new KeepAliveEnumeratePropertiesDelegate(KeepAliveEnumerateProperties);

            jsengine_registerManagedDels(
                nativeJsEnginePtr,
                _keepalive_remove,
                _keepalive_get_property_value,
                _keepalive_set_property_value,
                _keepalive_valueof,
                _keepalive_invoke,
                _keepalive_delete_property,
                _keepalive_enumerate_properties
                );
            _engine = new HandleRef(this, nativeJsEnginePtr);
            this.defaultTypeBuilder = defaultTypeBuilder;
        }
        public JsEngine(int maxYoungSpace, int maxOldSpace)
           : this(new DefaultJsTypeDefinitionBuilder(), maxYoungSpace, maxOldSpace)
        {

        }
        public JsEngine()
            : this(new DefaultJsTypeDefinitionBuilder(), -1, -1)
        {

        }
        internal HandleRef UnmanagedEngineHandler
        {
            get { return this._engine; }
        }

        public void TerminateExecution()
        {
            jsengine_terminate_execution(_engine);
        }

        public void DumpHeapStats()
        {
            jsengine_dump_heap_stats(_engine);
        }

        public void DisposeObject(IntPtr ptr)
        {
            // If the engine has already been explicitly disposed we pass Zero as
            // the first argument because we need to free the memory allocated by
            // "new" but not the object on the V8 heap: it has already been freed.
            if (_disposed)
                jsengine_dispose_object(new HandleRef(this, IntPtr.Zero), ptr);
            else
                jsengine_dispose_object(_engine, ptr);
        }

        void KeepAliveValueOf(int contextId, int slot, ref JsValue output)
        {
            JsContext context;
            if (!_aliveContexts.TryGetValue(contextId, out context))
            {

                throw new ContextNotFoundException(contextId);
            }
            context.KeepAliveGetValueOf(slot, ref output);
        }

        void KeepAliveInvoke(int contextId, int slot, ref JsValue args, ref JsValue output)
        {
            JsContext context;
            if (!_aliveContexts.TryGetValue(contextId, out context))
            {
                throw new ContextNotFoundException(contextId);
            }
            context.KeepAliveInvoke(slot, ref args, ref output);
        }
        void KeepAliveSetPropertyValue(int contextId, int slot, string name, ref JsValue value, ref JsValue output)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("set prop " + contextId + " " + slot);
#endif
            JsContext context;
            if (!_aliveContexts.TryGetValue(contextId, out context))
            {
                throw new ContextNotFoundException(contextId);
            }
            context.KeepAliveSetPropertyValue(slot, name, ref value, ref output);
        }
        void KeepAliveGetPropertyValue(int contextId, int slot, string name, ref JsValue output)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("get prop " + contextId + " " + slot);
#endif
            JsContext context;
            if (!_aliveContexts.TryGetValue(contextId, out context))
            {
                throw new ContextNotFoundException(contextId);
            }
            context.KeepAliveGetPropertyValue(slot, name, ref output);
        }
        void KeepAliveDeleteProperty(int contextId, int slot, string name, ref JsValue output)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("delete prop " + contextId + " " + slot);
#endif
            JsContext context;
            if (!_aliveContexts.TryGetValue(contextId, out context))
            {
                throw new ContextNotFoundException(contextId);
            }
            context.KeepAliveDeleteProperty(slot, name, ref output);
        }

        void KeepAliveEnumerateProperties(int contextId, int slot, ref JsValue output)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("enumerate props " + contextId + " " + slot);
#endif
            JsContext context;
            if (!_aliveContexts.TryGetValue(contextId, out context))
            {
                throw new ContextNotFoundException(contextId);
            }
            context.KeepAliveEnumerateProperties(slot, ref output);
        }
        void KeepAliveRemove(int contextId, int slot)
        {
#if DEBUG_TRACE_API
			Console.WriteLine("Keep alive remove for " + contextId + " " + slot);
#endif
            JsContext context;
            if (!_aliveContexts.TryGetValue(contextId, out context))
            {
                return;
            }
            context.KeepAliveRemove(slot);
        }

        public JsContext CreateContext()
        {
            CheckDisposed();
            //
            int newContextId = Interlocked.Increment(ref _currentContextId);
            JsContext ctx = new JsContext(newContextId, this, ContextDisposed, this.defaultTypeBuilder);

            _aliveContexts.Add(newContextId, ctx);
            return ctx;
        }
        public JsContext CreateContext(IntPtr nativeJsContext)
        {
            CheckDisposed();
            //
            int id = Interlocked.Increment(ref _currentContextId);
            JsContext ctx = new JsContext(id, this, ContextDisposed, nativeJsContext, this.defaultTypeBuilder);
            _aliveContexts.Add(id, ctx);
            return ctx;
        }
        public JsContext CreateContext(JsTypeDefinitionBuilder customTypeDefBuilder)
        {
            CheckDisposed();
            //
            int id = Interlocked.Increment(ref _currentContextId);
            JsContext ctx = new JsContext(id, this, ContextDisposed, customTypeDefBuilder);
            _aliveContexts.Add(id, ctx);
            return ctx;
        }
        public JsScript CompileScript(string code, string scriptName)
        {
            CheckDisposed();
            //
            int id = Interlocked.Increment(ref _currentScriptId);
            JsScript script = new JsScript(id,
                this,
                _engine,
                new JsConvert(null),
                code,
                scriptName,
                ScriptDisposed);

            _aliveScripts.Add(id, script);
            return script;
        }

        private void ContextDisposed(int id)
        {
            _aliveContexts.Remove(id);
        }

        private void ScriptDisposed(int id)
        {
            _aliveScripts.Remove(id);
        }

        //-------------------------------------------------
        bool _disposed;

        public bool IsDisposed
        {
            get { return _disposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            CheckDisposed();
            _disposed = true; //? here?

            if (disposing)
            {

                foreach (JsContext context in _aliveContexts.Values)
                {
                    context.Dispose();
                }
                _aliveContexts.Clear();
                //
                foreach (JsScript script in _aliveScripts.Values)
                {
                    script.Dispose();
                }
                _aliveScripts.Clear();
            }
#if DEBUG_TRACE_API
				Console.WriteLine("Calling jsEngine dispose: " + _engine.Handle.ToInt64());
#endif      
            jsengine_dispose(_engine);
        }

        void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("JsEngine:" + _engine.Handle);
        }


        public static void DumpAllocatedItems()
        {
            js_dump_allocated_items();
        }
        ~JsEngine()
        {
            if (!_disposed)
                Dispose(false);
        }

    }
}
