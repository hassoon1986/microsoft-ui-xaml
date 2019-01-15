﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using MUXControlsTestApp.Utilities;
using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading;
using System.Collections.Generic;
using Windows.UI.Xaml.Tests.MUXControls.ApiTests.RepeaterTests.Common.Mocks;
using System.Numerics;
using Common;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Tests.MUXControls.ApiTests.RepeaterTests.Common;

using Task = System.Threading.Tasks.Task;

#if USING_TAEF
using WEX.TestExecution;
using WEX.TestExecution.Markup;
using WEX.Logging.Interop;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
#endif

#if !BUILD_WINDOWS
using VirtualizingLayout = Microsoft.UI.Xaml.Controls.VirtualizingLayout;
using ItemsRepeater = Microsoft.UI.Xaml.Controls.ItemsRepeater;
using VirtualizingLayoutContext = Microsoft.UI.Xaml.Controls.VirtualizingLayoutContext;
using LayoutContext = Microsoft.UI.Xaml.Controls.LayoutContext;
using RecyclingElementFactory = Microsoft.UI.Xaml.Controls.RecyclingElementFactory;
using StackLayout = Microsoft.UI.Xaml.Controls.StackLayout;
using UniformGridLayout = Microsoft.UI.Xaml.Controls.UniformGridLayout;
using ScrollAnchorProvider = Microsoft.UI.Xaml.Controls.ScrollAnchorProvider;
using Scroller = Microsoft.UI.Xaml.Controls.Primitives.Scroller;
using ScrollerViewChangeCompletedEventArgs = Microsoft.UI.Xaml.Controls.ScrollerViewChangeCompletedEventArgs;
using ScrollerViewChangeSnapPointRespect = Microsoft.UI.Xaml.Controls.ScrollerViewChangeSnapPointRespect;
using ScrollerChangeOffsetsOptions = Microsoft.UI.Xaml.Controls.ScrollerChangeOffsetsOptions;
using ScrollerViewChangeKind = Microsoft.UI.Xaml.Controls.ScrollerViewChangeKind;
using ScrollerViewKind = Microsoft.UI.Xaml.Controls.ScrollerViewKind;
using ScrollerChangeZoomFactorOptions = Microsoft.UI.Xaml.Controls.ScrollerChangeZoomFactorOptions;
using IRepeaterScrollingSurface = Microsoft.UI.Private.Controls.IRepeaterScrollingSurface;
using ConfigurationChangedEventHandler = Microsoft.UI.Private.Controls.ConfigurationChangedEventHandler;
using PostArrangeEventHandler = Microsoft.UI.Private.Controls.PostArrangeEventHandler;
using ViewportChangedEventHandler = Microsoft.UI.Private.Controls.ViewportChangedEventHandler;
#endif


#if !BUILD_WINDOWS

namespace Windows.UI.Xaml.Tests.MUXControls.ApiTests.RepeaterTests
{
    [TestClass]
    public class ViewportTests : TestsBase
    {
        [TestMethod]
        public void ValidateNoScrollingSurfaceScenario()
        {
            RunOnUIThread.Execute(() =>
            {
                var realizationRects = new List<Rect>();

                var repeater = new ItemsRepeater()
                {
                    Layout = GetMonitoringLayout(new Size(500, 500), realizationRects),
                    HorizontalCacheLength = 0.0,
                    VerticalCacheLength = 0.0
                };

                Content = repeater;
                Content.UpdateLayout();

                Verify.AreEqual(2, realizationRects.Count);
                Verify.AreEqual(new Rect(0, 0, 0, 0), realizationRects[0]);

                if (!PlatformConfiguration.IsOsVersionGreaterThanOrEqual(OSVersion.Redstone5))
                {
                    Verify.AreEqual(new Rect(0, 0, float.MaxValue, float.MaxValue), realizationRects[1]);
                }
                else
                {
                    // Using Effective Viewport
                    Verify.AreEqual(0, realizationRects[1].X);
                    // 32 pixel title bar and some tolerance for borders
                    Verify.IsLessThan(2.0, Math.Abs(realizationRects[1].Y - 32));
                    // Width/Height depends on the window size, so just
                    // validating something reasonable here to avoid flakiness.
                    Verify.IsLessThan(500.0, realizationRects[1].Width);
                    Verify.IsLessThan(500.0, realizationRects[1].Height);
                }

                realizationRects.Clear();
            });
        }

        // [TestMethod] Temporarily disabled for bug 18866003
        public void ValidateScrollAnchorProviderScenario()
        {
            var realizationRects = new List<Rect>();
            var scrollViewer = (ScrollViewer)null;
            var viewChangedEvent = new ManualResetEvent(false);
            int waitTime = 2000; // 2 seconds 

            RunOnUIThread.Execute(() =>
            {
                var repeater = new ItemsRepeater()
                {
                    Layout = GetMonitoringLayout(new Size(500, 600), realizationRects),
                    HorizontalCacheLength = 0.0,
                    VerticalCacheLength = 0.0
                };

                scrollViewer = new ScrollViewer
                {
                    Content = repeater,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                };

                scrollViewer.ViewChanged += (sender, args) =>
                {
                    if (!args.IsIntermediate)
                    {
                        viewChangedEvent.Set();
                    }
                };

                var tracker = new ScrollAnchorProvider()
                {
                    Width = 200,
                    Height = 300,
                    Content = scrollViewer
                };

                Content = tracker;
                Content.UpdateLayout();

                // First layout pass will invalidate measure during the first arrange
                // so that we can get a viewport during the second measure/arrange pass.
                // That's why Measure gets invoked twice.
                // After that, ScrollViewer.SizeChanged is raised and it also invalidates
                // layout (third pass).
                Verify.AreEqual(3, realizationRects.Count);
                Verify.AreEqual(new Rect(0, 0, 0, 0), realizationRects[0]);
                Verify.AreEqual(new Rect(0, 0, 200, 300), realizationRects[1]);
                Verify.AreEqual(new Rect(0, 0, 200, 300), realizationRects[2]);
                realizationRects.Clear();

                viewChangedEvent.Reset();
                scrollViewer.ChangeView(null, 100.0, 1.0f, disableAnimation: true);
            });

            Verify.IsTrue(viewChangedEvent.WaitOne(waitTime), "Waiting for view changed");
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(new Rect(0, 100, 200, 300), realizationRects.Last());
                realizationRects.Clear();

                viewChangedEvent.Reset();
                // Max viewport offset is (300, 400). Horizontal viewport offset
                // is expected to get coerced from 400 to 300.
                scrollViewer.ChangeView(400, 100.0, 1.0f, disableAnimation: true);
            });

            Verify.IsTrue(viewChangedEvent.WaitOne(waitTime), "Waiting for view changed");
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(new Rect(300, 100, 200, 300), realizationRects.Last());
                realizationRects.Clear();

                viewChangedEvent.Reset();
                scrollViewer.ChangeView(null, null, 2.0f, disableAnimation: true);
            });

            Verify.IsTrue(viewChangedEvent.WaitOne(waitTime), "Waiting for view changed");
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(new Rect(300, 100, 100, 150), realizationRects.Last());
                realizationRects.Clear();
            });
        }

        //Unreliable test: RepeaterTests.ViewportTests.ValidateOneScrollerScenario #158
        //[TestMethod]
        public void ValidateOneScrollerScenario()
        {
            var realizationRects = new List<Rect>();
            var scroller = (Scroller)null;
            var viewChangeCompletedEvent = new AutoResetEvent(false);

            RunOnUIThread.Execute(() =>
            {
                var repeater = new ItemsRepeater()
                {
                    Layout = GetMonitoringLayout(new Size(500, 600), realizationRects),
                    HorizontalCacheLength = 0.0,
                    VerticalCacheLength = 0.0
                };

                scroller = new Scroller
                {
                    Child = repeater,
                    Width = 200,
                    Height = 300
                };

                Content = scroller;
                Content.UpdateLayout();

                Verify.AreEqual(2, realizationRects.Count);
                Verify.AreEqual(new Rect(0, 0, 0, 0), realizationRects[0]);
                Verify.AreEqual(new Rect(0, 0, 200, 300), realizationRects[1]);
                realizationRects.Clear();

                scroller.ViewChangeCompleted += (Scroller sender, ScrollerViewChangeCompletedEventArgs args) =>
                {
                    viewChangeCompletedEvent.Set();
                };
            });
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                scroller.ChangeOffsets(new ScrollerChangeOffsetsOptions(0.0, 100.0, ScrollerViewKind.Absolute, ScrollerViewChangeKind.DisableAnimation, ScrollerViewChangeSnapPointRespect.IgnoreSnapPoints));
            });
            Verify.IsTrue(viewChangeCompletedEvent.WaitOne(DefaultWaitTimeInMS));

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(new Rect(0, 100, 200, 300), realizationRects.Last());
                realizationRects.Clear();

                viewChangeCompletedEvent.Reset();
                // Max viewport offset is (300, 400). Horizontal viewport offset
                // is expected to get coerced from 400 to 300.
                scroller.ChangeOffsets(new ScrollerChangeOffsetsOptions(400.0, 100.0, ScrollerViewKind.Absolute, ScrollerViewChangeKind.DisableAnimation, ScrollerViewChangeSnapPointRespect.IgnoreSnapPoints));
            });
            Verify.IsTrue(viewChangeCompletedEvent.WaitOne(DefaultWaitTimeInMS));

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(new Rect(300, 100, 200, 300), realizationRects.Last());
                realizationRects.Clear();

                viewChangeCompletedEvent.Reset();
                scroller.ChangeZoomFactor(new ScrollerChangeZoomFactorOptions(2.0f, ScrollerViewKind.Absolute, Vector2.Zero, ScrollerViewChangeKind.DisableAnimation, ScrollerViewChangeSnapPointRespect.IgnoreSnapPoints));
            });
            Verify.IsTrue(viewChangeCompletedEvent.WaitOne(DefaultWaitTimeInMS));

            RunOnUIThread.Execute(() =>
            {
                // There are some known differences in InteractionTracker zoom between RS1 and RS2.
                Verify.AreEqual(
                    PlatformConfiguration.IsOsVersionGreaterThanOrEqual(OSVersion.Redstone2) ?
                    new Rect(300, 100, 100, 150) :
                    new Rect(150, 100, 100, 150),
                    realizationRects.Last());
                realizationRects.Clear();
            });
        }

        [TestMethod]
        public void ValidateTwoScrollersScenario()
        {
            var realizationRects = new List<Rect>();
            var horizontalScroller = (Scroller)null;
            var verticalScroller = (Scroller)null;
            var horizontalViewChangeCompletedEvent = new AutoResetEvent(false);
            var verticalViewChangeCompletedEvent = new AutoResetEvent(false);

            RunOnUIThread.Execute(() =>
            {
                var repeater = new ItemsRepeater()
                {
                    Layout = GetMonitoringLayout(new Size(500, 500), realizationRects),
                    HorizontalCacheLength = 0.0,
                    VerticalCacheLength = 0.0
                };

                horizontalScroller = new Scroller
                {
                    Child = repeater,
                    IsChildAvailableHeightConstrained = true
                };

                var grid = new Grid();
                grid.Children.Add(horizontalScroller);

                verticalScroller = new Scroller
                {
                    Child = grid,
                    Width = 200,
                    Height = 200,
                    IsChildAvailableWidthConstrained = true
                };

                Content = verticalScroller;
                Content.UpdateLayout();

                Verify.AreEqual(2, realizationRects.Count);
                Verify.AreEqual(new Rect(0, 0, 0, 0), realizationRects[0]);
                Verify.AreEqual(new Rect(0, 0, 200, 200), realizationRects[1]);
                realizationRects.Clear();

                horizontalScroller.ViewChangeCompleted += (Scroller sender, ScrollerViewChangeCompletedEventArgs args) =>
                {
                    horizontalViewChangeCompletedEvent.Set();
                };

                verticalScroller.ViewChangeCompleted += (Scroller sender, ScrollerViewChangeCompletedEventArgs args) =>
                {
                    verticalViewChangeCompletedEvent.Set();
                };
            });
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                verticalScroller.ChangeOffsets(new ScrollerChangeOffsetsOptions(0.0, 100.0, ScrollerViewKind.Absolute, ScrollerViewChangeKind.DisableAnimation, ScrollerViewChangeSnapPointRespect.IgnoreSnapPoints));
            });
            Verify.IsTrue(verticalViewChangeCompletedEvent.WaitOne(DefaultWaitTimeInMS));

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(new Rect(0, 100, 200, 200), realizationRects.Last());
                realizationRects.Clear();

                // Max viewport offset is (300, 300). Horizontal viewport offset
                // is expected to get coerced from 400 to 300.
                horizontalScroller.ChangeOffsets(new ScrollerChangeOffsetsOptions(400.0, 100.0, ScrollerViewKind.Absolute, ScrollerViewChangeKind.DisableAnimation, ScrollerViewChangeSnapPointRespect.IgnoreSnapPoints));
            });
            Verify.IsTrue(horizontalViewChangeCompletedEvent.WaitOne(DefaultWaitTimeInMS));

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(new Rect(300, 100, 200, 200), realizationRects.Last());
                realizationRects.Clear();
            });
        }

        [TestMethod]
        public void ValidateSupportForScrollerConfigurationChanges()
        {
            // In RS1, the interaction tracker always animates the viewport when its offset
            // is forced to change because the extent got smaller (e.g. viewport.x +  viewport.width > extent.width
            // will force the viewport offset to a smaller value).
            // In this test, we are not testing a behavior that's specific to an OS version,
            // so we will run this test in RS2+ to keep it simple.
            if(!PlatformConfiguration.IsOsVersionGreaterThanOrEqual(OSVersion.Redstone2))
            {
                Log.Warning("Skipping ValidateSupportForScrollerConfigurationChanges");
                return;
            }

            // Post RS4 configuration changes will not be raised.
            if (PlatformConfiguration.IsOsVersionGreaterThanOrEqual(OSVersion.Redstone5))
            {
                Log.Comment("Skipping since version is less than RS5 and effective viewport is not available below RS5");
                return;
            }

            // From the inner most to the outer most scroller.
            var scrollers = new Scroller[4];
            var grids = new Grid[4];
            var realizationRects = new List<Rect>();
            var viewChangeCompletedEvent = new AutoResetEvent(false);

            RunOnUIThread.Execute(() =>
            {
                var repeater = new ItemsRepeater()
                {
                    Layout = GetMonitoringLayout(new Size(500, 500), realizationRects),
                    HorizontalCacheLength = 0.0,
                    VerticalCacheLength = 0.0
                };

                for (int i = 0; i < scrollers.Length; ++i)
                {
                    grids[i] = new Grid();
                    grids[i].Children.Add(i > 0 ? (UIElement)scrollers[i - 1] : repeater);
                    
                    scrollers[i] = new Scroller()
                    {
                        Child = grids[i],
                        IsChildAvailableWidthConstrained = true,
                        IsChildAvailableHeightConstrained = true
                    };
                }

                var outerScroller = scrollers.Last();
                outerScroller.Width = 200.0;
                outerScroller.Height = 200.0;

                Content = outerScroller;
                Content.UpdateLayout();

                Verify.AreEqual(2, realizationRects.Count);
                Verify.AreEqual(new Rect(0, 0, 0, 0), realizationRects[0]);
                Verify.AreEqual(new Rect(0, 0, float.MaxValue, float.MaxValue), realizationRects[1]);
                realizationRects.Clear();

                for (int i = 0; i < scrollers.Length; ++i)
                {
                    scrollers[i].ViewChangeCompleted += (Scroller sender, ScrollerViewChangeCompletedEventArgs args) =>
                    {
                        viewChangeCompletedEvent.Set();
                    };
                }
            });

            foreach (ScrollOrientation scrollOrientation in Enum.GetValues(typeof(ScrollOrientation)))
            {
                RunOnUIThread.Execute(() =>
                {
                    scrollers[1].IsChildAvailableHeightConstrained = (scrollOrientation == ScrollOrientation.Horizontal);
                    scrollers[2].IsChildAvailableWidthConstrained = (scrollOrientation == ScrollOrientation.Vertical);
                });
                IdleSynchronizer.Wait();

                RunOnUIThread.Execute(() =>
                {
                    viewChangeCompletedEvent.Reset();
                    if (scrollOrientation == ScrollOrientation.Vertical)
                    {
                        scrollers[1].ChangeOffsets(new ScrollerChangeOffsetsOptions(0.0, 100.0, ScrollerViewKind.Absolute, ScrollerViewChangeKind.DisableAnimation, ScrollerViewChangeSnapPointRespect.IgnoreSnapPoints));
                    }
                    else
                    {
                        scrollers[2].ChangeOffsets(new ScrollerChangeOffsetsOptions(150.0, 0.0, ScrollerViewKind.Absolute, ScrollerViewChangeKind.DisableAnimation, ScrollerViewChangeSnapPointRespect.IgnoreSnapPoints)); 
                    }
                });
                Verify.IsTrue(viewChangeCompletedEvent.WaitOne(DefaultWaitTimeInMS));

                RunOnUIThread.Execute(() =>
                {
                    if (scrollOrientation == ScrollOrientation.Vertical)
                    {
                        Verify.AreEqual(new Rect(0, 100, 200, 200), realizationRects.Last());
                    }
                    else
                    {
                        Verify.AreEqual(new Rect(150, 0, 200, 200), realizationRects.Last());
                    }

                    realizationRects.Clear();
                });
            }
        }

        [TestMethod]
        public void CanGrowCacheBuffer()
        {
            var scroller = (Scroller)null;
            var repeater = (ItemsRepeater)null;
            var measureRealizationRects = new List<Rect>();
            var arrangeRealizationRects = new List<Rect>();
            var fullCacheEvent = new ManualResetEvent(initialState: false);

            RunOnUIThread.Execute(() =>
            {
                Log.Comment("Preparing the visual tree...");

                scroller = new Scroller
                {
                    Width = 400,
                    Height = 400
                };

                var layout = new MockVirtualizingLayout
                {
                    MeasureLayoutFunc = (availableSize, context) =>
                    {
                        measureRealizationRects.Add(context.RealizationRect);
                        return new Size(1000, 2000);
                    },

                    ArrangeLayoutFunc = (finalSize, context) =>
                    {
                        arrangeRealizationRects.Add(context.RealizationRect);

                        if (context.RealizationRect.Height == scroller.Height * (repeater.VerticalCacheLength + 1))
                        {
                            fullCacheEvent.Set();
                        }

                        return finalSize;
                    }
                };

                repeater = new ItemsRepeater()
                {
                    Layout = layout
                };

                scroller.Child = repeater;
                Content = scroller;
            });

            if (!fullCacheEvent.WaitOne(500000)) Verify.Fail("Cache full size never reached.");
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                var cacheLength = repeater.VerticalCacheLength;
                var expectedVisibleWindow = new Rect(0, 0, scroller.Width, scroller.Height);
                var expectedRealizationWindow = new Rect(
                    -cacheLength / 2 * scroller.Width,
                    -cacheLength / 2 * scroller.Height,
                    (1 + cacheLength) * scroller.Width,
                    (1 + cacheLength) * scroller.Height);

                Log.Comment("Validate that the realization window reached full size.");
                Verify.AreEqual(expectedRealizationWindow, measureRealizationRects.Last());

                Verify.AreEqual(expectedRealizationWindow, arrangeRealizationRects.Last());

                Log.Comment("Validate that the realization window grew by 40 pixels each time during the process.");
                for (int i = 2; i < measureRealizationRects.Count; ++i)
                {
                    Verify.AreEqual(-40, measureRealizationRects[i].X - measureRealizationRects[i - 1].X);
                    Verify.AreEqual(-40, measureRealizationRects[i].Y - measureRealizationRects[i - 1].Y);
                    Verify.AreEqual(80, measureRealizationRects[i].Width - measureRealizationRects[i - 1].Width);
                    Verify.AreEqual(80, measureRealizationRects[i].Height - measureRealizationRects[i - 1].Height);

                    Verify.AreEqual(-40, arrangeRealizationRects[i].X - arrangeRealizationRects[i - 1].X);
                    Verify.AreEqual(-40, arrangeRealizationRects[i].Y - arrangeRealizationRects[i - 1].Y);
                    Verify.AreEqual(80, arrangeRealizationRects[i].Width - arrangeRealizationRects[i - 1].Width);
                    Verify.AreEqual(80, arrangeRealizationRects[i].Height - arrangeRealizationRects[i - 1].Height);
                }
            });
        }

        [TestMethod]
        public void CanRegisterElementsWithScrollingSurfaces()
        {
            if (PlatformConfiguration.IsOsVersionGreaterThanOrEqual(OSVersion.Redstone5))
            {
                Log.Warning("Skipping since RS5+ we use effective viewport instead of IRepeaterScrollingSurface");
                return;
            }

            // In this test, we validate that ItemsRepeater can register and unregister its children
            // with one or two scrollers.
            // The initial setup is 4 nested scrollers with a root repeater under which
            // we have two groups (and two other inner repeaters).
            RunElementTrackingTestRoutine((data, scrollers, rootRepeater) =>
            {
                var resetScrollers = (Action)(() =>
                {
                    foreach (var scroller in scrollers)
                    {
                        scroller.IsHorizontallyScrollable = false;
                        scroller.IsVerticallyScrollable = false;
                        scroller.RegisterAnchorCandidateFunc = null;
                        scroller.UnregisterAnchorCandidateFunc = null;
                        scroller.GetRelativeViewportFunc = null;
                    }
                });

                var actualActionSequence = new List<string>();
                var expectedActionSequence = new List<string>()
                {
                    "S2: GetRelativeViewport ItemsRepeater #0",
                    "S2: Register Item #0.0",
                    "S2: Register Item #0.1",
                    "S2: Register Item #0.2",
                    "S2: GetRelativeViewport ItemsRepeater #1",
                    "S2: Register Item #1.0",
                    "S2: Register Item #1.1",
                    "S2: Register Item #1.2",
                    "S2: GetRelativeViewport Root ItemsRepeater",
                    "S2: Register Group #0",
                    "S2: Register Group #1"
                };

                var registerAnchorCandidateFunc = (Action<TestScrollingSurface, UIElement, bool>)((scroller, element, expectedInPostArrange) =>
                {
                    actualActionSequence.Add(scroller.Name + ": Register " + ((FrameworkElement)element).Name);
                    Log.Comment(actualActionSequence.Last());
                    Verify.AreEqual(expectedInPostArrange, scroller.InPostArrange);
                });

                var unregisterAnchorCandidateFunc = (Action<TestScrollingSurface, UIElement>)((scroller, element) =>
                {
                    actualActionSequence.Add(scroller.Name + ": Unregister " + ((FrameworkElement)element).Name);
                    Log.Comment(actualActionSequence.Last());
                    Verify.IsFalse(scroller.InArrange);
                    Verify.IsFalse(scroller.InPostArrange);
                });

                var getRelativeViewportFunc = (Func<TestScrollingSurface, UIElement, bool, Rect>)((scroller, element, expectedInPostArrange) =>
                {
                    actualActionSequence.Add(scroller.Name + ": GetRelativeViewport " + ((FrameworkElement)element).Name);
                    Log.Comment(actualActionSequence.Last());
                    Verify.AreEqual(expectedInPostArrange, scroller.InPostArrange);
                    var outerScroller = scrollers.Last();
                    return new Rect(0, 0, outerScroller.Width, outerScroller.Height);
                });

                // Step 1.0:
                // - Make scroller 2 scrollable in both directions.
                // - Validate that the correct methods are called on it from repeater at the right moment.
                scrollers[2].IsHorizontallyScrollable = true;
                scrollers[2].IsVerticallyScrollable = true;
                scrollers[2].RegisterAnchorCandidateFunc = (element) => registerAnchorCandidateFunc(scrollers[2], element, true);
                scrollers[2].UnregisterAnchorCandidateFunc = (element) => unregisterAnchorCandidateFunc(scrollers[2], element);
                scrollers[2].GetRelativeViewportFunc = (element) => getRelativeViewportFunc(scrollers[2], element, true);

                Content.UpdateLayout();
                Verify.AreEqual(string.Join(", ", expectedActionSequence.Concat(expectedActionSequence)), string.Join(", ", actualActionSequence));
                actualActionSequence.Clear();

                // Step 1.1:
                // - Remove an item from the data.
                // - Validate that the recycled element is no longer a candidate for tracking.
                data[0].RemoveAt(1);
                Content.UpdateLayout();
                Verify.AreEqual(string.Join( ", ",
                    new List<string>()
                    {
                        "S2: Unregister Item #0.1",
                        "S2: GetRelativeViewport ItemsRepeater #0",
                        "S2: Register Item #0.0",
                        "S2: Register Item #0.2",
                        "S2: GetRelativeViewport ItemsRepeater #1",
                        "S2: Register Item #1.0",
                        "S2: Register Item #1.1",
                        "S2: Register Item #1.2",
                        "S2: GetRelativeViewport Root ItemsRepeater",
                        "S2: Register Group #0",
                        "S2: Register Group #1"
                    }),
                    string.Join(", ", actualActionSequence));
                actualActionSequence.Clear();

                // Step 2.0:
                // - Reset the scrollers configuration.
                // - Configure scroller 1 and 3 to be, respectively, vertically and horizontally scrollable.
                // - Validate that the correct methods are called on scroller 1 and 3 from repeater at the right moment.
                resetScrollers();
                scrollers[1].IsVerticallyScrollable = true;
                scrollers[3].IsHorizontallyScrollable = true;

                scrollers[1].RegisterAnchorCandidateFunc = (element) => registerAnchorCandidateFunc(scrollers[1], element, false);
                scrollers[1].UnregisterAnchorCandidateFunc = (element) => unregisterAnchorCandidateFunc(scrollers[1], element);
                scrollers[1].GetRelativeViewportFunc = (element) => getRelativeViewportFunc(scrollers[1], element, false);

                scrollers[3].RegisterAnchorCandidateFunc = (element) => registerAnchorCandidateFunc(scrollers[3], element, true);
                scrollers[3].UnregisterAnchorCandidateFunc = (element) => unregisterAnchorCandidateFunc(scrollers[3], element);
                scrollers[3].GetRelativeViewportFunc = (element) => getRelativeViewportFunc(scrollers[3], element, true);

                Content.UpdateLayout();
                Verify.AreEqual(string.Join(", ",
                    new List<string>()
                    {
                        "S3: GetRelativeViewport ItemsRepeater #0",
                        "S1: GetRelativeViewport ItemsRepeater #0",
                        "S3: Register Item #0.0",
                        "S1: Register Item #0.0",
                        "S3: Register Item #0.2",
                        "S1: Register Item #0.2",
                        "S3: GetRelativeViewport ItemsRepeater #1",
                        "S1: GetRelativeViewport ItemsRepeater #1",
                        "S3: Register Item #1.0",
                        "S1: Register Item #1.0",
                        "S3: Register Item #1.1",
                        "S1: Register Item #1.1",
                        "S3: Register Item #1.2",
                        "S1: Register Item #1.2",
                        "S3: GetRelativeViewport Root ItemsRepeater",
                        "S1: GetRelativeViewport Root ItemsRepeater",
                        "S3: Register Group #0",
                        "S1: Register Group #0",
                        "S3: Register Group #1",
                        "S1: Register Group #1"
                    }),
                    string.Join(", ", actualActionSequence));
                actualActionSequence.Clear();

                // Step 2.1:
                // - Remove an item from the data.
                // - Validate that scroller 1 and 3 are no longer tracking the recycled element because it's not registered anymore.
                data[1].RemoveAt(1);
                Content.UpdateLayout();
                Verify.AreEqual(string.Join(", ",
                    new List<string>()
                    {
                        "S3: Unregister Item #1.1",
                        "S1: Unregister Item #1.1",
                        "S3: GetRelativeViewport ItemsRepeater #0",
                        "S1: GetRelativeViewport ItemsRepeater #0",
                        "S3: Register Item #0.0",
                        "S1: Register Item #0.0",
                        "S3: Register Item #0.2",
                        "S1: Register Item #0.2",
                        "S3: GetRelativeViewport ItemsRepeater #1",
                        "S1: GetRelativeViewport ItemsRepeater #1",
                        "S3: Register Item #1.0",
                        "S1: Register Item #1.0",
                        "S3: Register Item #1.2",
                        "S1: Register Item #1.2",
                        "S3: GetRelativeViewport Root ItemsRepeater",
                        "S1: GetRelativeViewport Root ItemsRepeater",
                        "S3: Register Group #0",
                        "S1: Register Group #0",
                        "S3: Register Group #1",
                        "S1: Register Group #1"
                    }),
                    string.Join(", ", actualActionSequence));
                actualActionSequence.Clear();
                //Log.Comment(">> " + string.Join(", ", actualActionSequence.Select(i => "\"" + i + "\"")));
            });
        }

        [TestMethod]
        public void ValidateSuggestedElement()
        {
            if (PlatformConfiguration.IsOsVersionGreaterThanOrEqual(OSVersion.Redstone5))
            {
                Log.Warning("Skipping since RS5+ we use effective viewport instead of IRepeaterScrollingSurface");
                return;
            }

            // In this test, we validate that ItemsRepeater can suggested the correct anchor element
            // to its layout.
            // The initial setup is 4 nested scrollers with a root repeater under which
            // we have two groups (and two other inner repeaters).
            RunElementTrackingTestRoutine((data, scrollers, rootRepeater) =>
            {
                var outerScroller = scrollers[3];
                scrollers[3].IsVerticallyScrollable = true;
                scrollers[1].IsHorizontallyScrollable = true;

                foreach(var scrollableScroller in new [] { scrollers[1], scrollers[3] })
                {
                    scrollableScroller.RegisterAnchorCandidateFunc = (element) => { Log.Comment("Register {0}", ((FrameworkElement)element).Name); };
                    scrollableScroller.UnregisterAnchorCandidateFunc = (element) => { Log.Comment("Unregister {0}", ((FrameworkElement)element).Name); };
                    scrollableScroller.GetRelativeViewportFunc = (element) => { Log.Comment("GetRelativeViewport {0}", ((FrameworkElement)element).Name); return new Rect(0, 0, outerScroller.Width, outerScroller.Height); };
                }                

                var groups = Enumerable.Range(0, data.Count).Select(i =>
                {
                    var group = (StackPanel)rootRepeater.TryGetElement(i);
                    return new
                    {
                        Group = group,
                        Header = (TextBlock)group.Children[0],
                        ItemsRepeater = (ItemsRepeater)group.Children[1]
                    };
                }).ToArray();

                // Scroller 3 will be tracking the first group header while scroller 1 will
                // be tracking the second group header. Our repeaters (root repeater and the two inner
                // "group" repeaters) should only care about what scroller 1 is tracking because
                // it's the inner most scrollable scroller.
                scrollers[3].AnchorElement = groups[0].Header;
                scrollers[1].AnchorElement = groups[1].Header;
                Content.UpdateLayout();

                var stackLayout = (TestStackLayout)rootRepeater.Layout;
                var gridLayout1 = (TestGridLayout)groups[0].ItemsRepeater.Layout;
                var gridLayout2 = (TestGridLayout)groups[1].ItemsRepeater.Layout;

                // Root repeater should suggest the second group (because the group header is
                // an indirect child of it).
                // The other repeaters should not suggest anything because the anchor element
                // is not one of their children (either directly or indirectly).
                Verify.AreEqual(groups[1].Group, stackLayout.SuggestedAnchor);
                Verify.IsNull(gridLayout1.SuggestedAnchor);
                Verify.IsNull(gridLayout2.SuggestedAnchor);

                // Now let's make the second element in the second inner repeater the anchor element.
                scrollers[1].AnchorElement = groups[1].ItemsRepeater.TryGetElement(1);
                rootRepeater.InvalidateMeasure();
                groups[0].ItemsRepeater.InvalidateMeasure();
                groups[1].ItemsRepeater.InvalidateMeasure();
                Content.UpdateLayout();

                // Root repeater should suggest the second group like before.
                // The second inner repeater will suggest the anchor element because it's a direct child.
                Verify.AreEqual(groups[1].Group, stackLayout.SuggestedAnchor);
                Verify.IsNull(gridLayout1.SuggestedAnchor);
                Verify.AreEqual(scrollers[1].AnchorElement, gridLayout2.SuggestedAnchor);
            });
        }

        // [TestMethod] Temporarily disabled for bug 18866003
        public void ValidateLoadUnload()
        {
            if (!PlatformConfiguration.IsOsVersionGreaterThan(OSVersion.Redstone2))
            {
                Log.Warning("Skipping: Test has instability on rs1 and rs2. Tracked by bug 18919096");
                return;
            }

            // In this test, we will repeatedly put a repeater in and out
            // of the visual tree, under the same or a different parent.
            // And we will validate that the subscriptions and unsubscriptions to
            // the IRepeaterScrollingSurface events is done in sync.

            TestScrollingSurface scroller1 = null;
            TestScrollingSurface scroller2 = null;
            ItemsRepeater repeater = null;
            WeakReference repeaterWeakRef = null;
            var renderingEvent = new ManualResetEvent(false);

            var unorderedLoadEvent = false;
            var loadCounter = 0;
            var unloadCounter = 0;

            int scroller1SubscriberCount = 0;
            int scroller2SubscriberCount = 0;

            RunOnUIThread.Execute(() =>
            {
                CompositionTarget.Rendering += (sender, args) =>
                {
                    renderingEvent.Set();
                };

                var host = new Grid();
                scroller1 = new TestScrollingSurface() { Name = "Scroller 1" };
                scroller2 = new TestScrollingSurface() { Name = "Scroller 2" };
                repeater = new ItemsRepeater();
                repeaterWeakRef = new WeakReference(repeater);

                repeater.Loaded += delegate
                {
                    Log.Comment("ItemsRepeater Loaded in " + ((FrameworkElement)repeater.Parent).Name);
                    unorderedLoadEvent |= (++loadCounter > unloadCounter + 1);
                };
                repeater.Unloaded += delegate
                {
                    Log.Comment("ItemsRepeater Unloaded");
                    unorderedLoadEvent |= (++unloadCounter > loadCounter);
                };

                // Subscribers count should never go above 1 or under 0.
                var validateSubscriberCount = new Action(() =>
                {
                    Verify.IsLessThanOrEqual(scroller1SubscriberCount, 1);
                    Verify.IsGreaterThanOrEqual(scroller1SubscriberCount, 0);

                    Verify.IsLessThanOrEqual(scroller2SubscriberCount, 1);
                    Verify.IsGreaterThanOrEqual(scroller2SubscriberCount, 0);
                });

                scroller1.ConfigurationChangedAddFunc = () => { ++scroller1SubscriberCount; validateSubscriberCount(); };
                scroller2.ConfigurationChangedAddFunc = () => { ++scroller2SubscriberCount; validateSubscriberCount(); };
                scroller1.ConfigurationChangedRemoveFunc = () => { --scroller1SubscriberCount; validateSubscriberCount(); };
                scroller2.ConfigurationChangedRemoveFunc = () => { --scroller2SubscriberCount; validateSubscriberCount(); };

                scroller1.Content = repeater;
                host.Children.Add(scroller1);
                host.Children.Add(scroller2);

                Content = host;
            });

            IdleSynchronizer.Wait();
            Verify.IsTrue(renderingEvent.WaitOne(), "Waiting for rendering event");

            renderingEvent.Reset();
            Log.Comment("Putting repeater in and out of scroller 1 until we observe two out-of-sync loaded/unloaded events.");
            for (int i = 0; i < 2; ++i)
            {
                while (!unorderedLoadEvent)
                {
                    RunOnUIThread.Execute(() =>
                    {
                        // Validate subscription count for events + reset.
                        scroller1.Content = null;
                        scroller1.Content = repeater;
                    });
                    // For this issue to repro, we need to wait in such a way
                    // that we don't tick the UI thread. We can't use IdleSynchronizer here.
                    Task.Delay(16 * 3).Wait();
                }
                unorderedLoadEvent = false;
                Log.Comment("Detected an unordered load/unload event.");
            }

            IdleSynchronizer.Wait();
            Verify.IsTrue(renderingEvent.WaitOne(), "Waiting for rendering event");

            renderingEvent.Reset();
            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(1, scroller1SubscriberCount);
                Verify.AreEqual(0, scroller2SubscriberCount);

                Log.Comment("Moving repeater from scroller 1 to scroller 2.");
                scroller1.Content = null;
                scroller2.Content = repeater;
            });

            IdleSynchronizer.Wait();
            Verify.IsTrue(renderingEvent.WaitOne(), "Waiting for rendering event");

            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(0, scroller1SubscriberCount);
                Verify.AreEqual(1, scroller2SubscriberCount);

                Log.Comment("Moving repeater out of scroller 2.");
                scroller2.Content = null;
                repeater = null;
            });

            Log.Comment("Waiting for repeater to get GCed.");
            for (int i = 0; i < 5 && repeaterWeakRef.IsAlive; ++i)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                IdleSynchronizer.Wait();
            }
            Verify.IsFalse(repeaterWeakRef.IsAlive);

            renderingEvent.Reset();
            RunOnUIThread.Execute(() =>
            {
                Verify.AreEqual(0, scroller1SubscriberCount);
                Verify.AreEqual(0, scroller2SubscriberCount);

                Log.Comment("Scroller raise IRepeaterScrollSurface.PostArrange. Make sure no one is subscribing to it.");
                scroller1.InvalidateArrange();
                scroller2.InvalidateArrange();
                Content.UpdateLayout();
            });

            IdleSynchronizer.Wait();
            Verify.IsTrue(renderingEvent.WaitOne(), "Waiting for rendering event");
        }

        // Test is flaky - disabling it while debugging the issue.
        // Bug 17581054: RepeaterTests.ViewportTests.CanBringIntoViewElements is failing on RS4 
        // [TestMethod]
        public void CanBringIntoViewElements()
        {
            if (!PlatformConfiguration.IsOsVersionGreaterThan(OSVersion.Redstone3))
            {
                Log.Warning("Skipping CanBringIntoViewElements because UIElement.BringIntoViewRequested was added in RS4.");
                return;
            }

            Scroller scroller = null;
            ItemsRepeater repeater = null;
            var rootLoadedEvent = new AutoResetEvent(initialState: false);
            var viewChangeCompletedEvent = new AutoResetEvent(initialState: false);
            var viewChangedOffsets = new List<double>();

            RunOnUIThread.Execute(() =>
            {
                var lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam laoreet erat vel massa rutrum, eget mollis massa vulputate. Vivamus semper augue leo, eget faucibus nulla mattis nec. Donec scelerisque lacus at dui ultricies, eget auctor ipsum placerat. Integer aliquet libero sed nisi eleifend, nec rutrum arcu lacinia. Sed a sem et ante gravida congue sit amet ut augue. Donec quis pellentesque urna, non finibus metus. Proin sed ornare tellus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam laoreet erat vel massa rutrum, eget mollis massa vulputate. Vivamus semper augue leo, eget faucibus nulla mattis nec. Donec scelerisque lacus at dui ultricies, eget auctor ipsum placerat. Integer aliquet libero sed nisi eleifend, nec rutrum arcu lacinia. Sed a sem et ante gravida congue sit amet ut augue. Donec quis pellentesque urna, non finibus metus. Proin sed ornare tellus.";
                var root = (Grid)XamlReader.Load(TestUtilities.ProcessTestXamlForRepo(
                     @"<Grid xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' xmlns:controls='using:Microsoft.UI.Xaml.Controls'> 
                         <Grid.Resources>
                           <controls:StackLayout x:Name='VerticalStackLayout' />
                           <controls:RecyclingElementFactory x:Key='ElementFactory'>
                             <controls:RecyclingElementFactory.RecyclePool>
                               <controls:RecyclePool />
                             </controls:RecyclingElementFactory.RecyclePool>
                             <DataTemplate x:Key='ItemTemplate'>
                               <Border Background='LightGray' Margin ='5'>
                                 <TextBlock Text='{Binding}' TextWrapping='WrapWholeWords' />
                               </Border>
                             </DataTemplate>
                             <DataTemplate x:Key='GroupTemplate'>
                               <StackPanel>
                                 <TextBlock Text='{Binding Name}' FontSize='24' Foreground='White' />
                                 <controls:ItemsRepeater
                                   x:Name='InnerRepeater'
                                   ItemsSource='{Binding}'
                                   ElementFactory='{StaticResource ElementFactory}'
                                   Layout='{StaticResource VerticalStackLayout}'
                                   HorizontalCacheLength='0'
                                   VerticalCacheLength='0' />
                               </StackPanel>
                             </DataTemplate>
                           </controls:RecyclingElementFactory>
                         </Grid.Resources>
                         <controls:Scroller x:Name='Scroller' Width='400' Height='600' IsChildAvailableWidthConstrained='True' Background='Gray'>
                           <controls:ItemsRepeater
                             x:Name='ItemsRepeater'
                             ElementFactory='{StaticResource ElementFactory}'
                             Layout='{StaticResource VerticalStackLayout}'
                             HorizontalCacheLength='0'
                             VerticalCacheLength='0' />
                         </controls:Scroller>
                       </Grid>"));

                var elementFactory = (RecyclingElementFactory)root.Resources["ElementFactory"];
                scroller = (Scroller)root.FindName("Scroller");
                repeater = (ItemsRepeater)root.FindName("ItemsRepeater");

                var groups = Enumerable.Range(0, 10).Select(i => new NamedGroup<string>(
                    "Group #" + i,
                    Enumerable.Range(0, 15).Select(j => string.Format("{0}.{1}: {2}", i, j, lorem.Substring(0, 250))))).ToList();

                repeater.ItemsSource = groups;

                elementFactory.SelectTemplateKey += (s, e) =>
                {
                    e.TemplateKey =
                        (e.DataContext is NamedGroup<string>) ?
                        "GroupTemplate" :
                        "ItemTemplate";
                };

                scroller.ViewChanged += (o, e) =>
                {
                    Log.Comment("ViewChanged: " + scroller.VerticalOffset);
                    viewChangedOffsets.Add(scroller.VerticalOffset);
                };

                ((IRepeaterScrollingSurface)scroller).ViewportChanged += (o, isFinal) =>
                {
                    if (isFinal)
                    {
                        viewChangeCompletedEvent.Set();
                    }
                };

                Content = root;

                root.Loaded += delegate
                {
                    rootLoadedEvent.Set();
                };
            });
            Verify.IsTrue(rootLoadedEvent.WaitOne(DefaultWaitTimeInMS));
            IdleSynchronizer.Wait();

            RunOnUIThread.Execute(() =>
            {
                Log.Comment("Scroll into view item the last item.");
                var group = (FrameworkElement)repeater.GetOrCreateElement(9);
                var innerRepeater = (ItemsRepeater)group.FindName("InnerRepeater");
                innerRepeater.GetOrCreateElement(14).StartBringIntoView();
            });
            Verify.IsTrue(viewChangeCompletedEvent.WaitOne(DefaultWaitTimeInMS));
            IdleSynchronizer.Wait();
            Verify.AreEqual(1, viewChangedOffsets.Count);
            viewChangedOffsets.Clear();
            ValidateRealizedRange(repeater, 8, 9, 9, 8, 9, 14);

            RunOnUIThread.Execute(() =>
            {
                Log.Comment("Scroll into view item 9.10 (already realized) w/ animation.");
                var group = (FrameworkElement)repeater.TryGetElement(9);
                var innerRepeater = (ItemsRepeater)group.FindName("InnerRepeater");
                innerRepeater.TryGetElement(10).StartBringIntoView(new BringIntoViewOptions
                {
                    VerticalAlignmentRatio = 0.5,
                    AnimationDesired = true
                });
            });
            Verify.IsTrue(viewChangeCompletedEvent.WaitOne(DefaultWaitTimeInMS));
            IdleSynchronizer.Wait();
            Verify.IsLessThan(1, viewChangedOffsets.Count);
            viewChangedOffsets.Clear();
            ValidateRealizedRange(repeater, 8, 9, 9, 6, 9, 14);

            Log.Comment("Validate no-op StartBringIntoView operations.");
            {
                var expectedVerticalOffsetAt9_10 = double.NaN;

                RunOnUIThread.Execute(() =>
                {
                    expectedVerticalOffsetAt9_10 = scroller.VerticalOffset;
                    var group = (FrameworkElement)repeater.GetOrCreateElement(9);
                    var innerRepeater = (ItemsRepeater)group.FindName("InnerRepeater");
                    innerRepeater.GetOrCreateElement(10).StartBringIntoView(new BringIntoViewOptions
                    {
                        VerticalAlignmentRatio = 0.5,
                        AnimationDesired = true
                    });
                });
                Verify.IsTrue(viewChangeCompletedEvent.WaitOne(DefaultWaitTimeInMS));
                IdleSynchronizer.Wait();

                RunOnUIThread.Execute(() =>
                {
                    Verify.AreEqual(expectedVerticalOffsetAt9_10, scroller.VerticalOffset);
                });
            }

            RunOnUIThread.Execute(() =>
            {
                Log.Comment("Scroll to item 3.4.");
                var group = (FrameworkElement)repeater.GetOrCreateElement(3);
                var innerRepeater = (ItemsRepeater)group.FindName("InnerRepeater");
                innerRepeater.GetOrCreateElement(4).StartBringIntoView(new BringIntoViewOptions
                {
                    VerticalAlignmentRatio = 0.0
                });
            });

            Verify.IsTrue(viewChangeCompletedEvent.WaitOne(DefaultWaitTimeInMS));
            IdleSynchronizer.Wait();
            ValidateRealizedRange(repeater, 2, 4, 3, 3, 3, 10);

            // Code defect bug 17711793
            //RunOnUIThread.Execute(() =>
            //{
            //    Log.Comment("Scroll 0.0 to the top");
            //    var group = (FrameworkElement)repeater.MakeAnchor(0);
            //    var innerRepeater = (ItemsRepeater)group.FindName("InnerRepeater");
            //    innerRepeater.MakeAnchor(0).StartBringIntoView(new BringIntoViewOptions
            //    {
            //        VerticalAlignmentRatio = 0.0,
            //        AnimationDesired = true
            //    });
            //});

            //Verify.IsTrue(viewChangeCompletedEvent.WaitOne(DefaultWaitTimeInMS));
            //IdleSynchronizer.Wait();
            //Verify.AreEqual(1, viewChangedOffsets.Count);   // Animations are always disabled for anchors outside the realized range.
            //viewChangedOffsets.Clear();
            //ValidateRealizedRange(repeater, 0, 1, 0, 0, 0, 6);
        }

        private void ValidateRealizedRange(
            ItemsRepeater repeater,
            int expectedFirstGroupIndex,
            int expectedLastGroupIndex,
            int expectedFirstItemGroupIndex,
            int expectedFirstItemIndex,
            int expectedLastItemGroupIndex,
            int expectedLastItemIndex)
        {
            int actualFirstGroupIndex = -1;
            int actualLastGroupIndex = -1;
            int actualFirstItemGroupIndex = -1;
            int actualFirstItemIndex = -1;
            int actualLastItemGroupIndex = -1;
            int actualLastItemIndex = -1;

            RunOnUIThread.Execute(() =>
            {
                int groupIndex = 0;
                int itemIndex = 0;

                var groups = (IEnumerable<NamedGroup<string>>)repeater.ItemsSource;

                foreach (var group in groups)
                {
                    var groupElement = repeater.TryGetElement(groupIndex);

                    if (groupElement != null)
                    {
                        actualFirstGroupIndex =
                            actualFirstGroupIndex == -1 ?
                            groupIndex :
                            actualFirstGroupIndex;

                        actualLastGroupIndex = groupIndex;

                        var innerRepeater = (ItemsRepeater)((FrameworkElement)groupElement).FindName("InnerRepeater");

                        foreach (var item in group)
                        {
                            var itemElement = innerRepeater.TryGetElement(itemIndex);

                            if(itemElement != null)
                            {
                                actualFirstItemGroupIndex =
                                    actualFirstItemGroupIndex == -1 ?
                                    groupIndex :
                                    actualFirstItemGroupIndex;

                                actualFirstItemIndex =
                                    actualFirstItemIndex == -1 ?
                                    itemIndex :
                                    actualFirstItemIndex;

                                actualLastItemGroupIndex = groupIndex;
                                actualLastItemIndex = itemIndex;
                            }

                            ++itemIndex;
                        }
                    }

                    itemIndex = 0;
                    ++groupIndex;
                }
            });

            Verify.AreEqual(expectedFirstGroupIndex, actualFirstGroupIndex);
            Verify.AreEqual(expectedLastGroupIndex, actualLastGroupIndex);
            Verify.AreEqual(expectedFirstItemGroupIndex, actualFirstItemGroupIndex);
            Verify.AreEqual(expectedFirstItemIndex, actualFirstItemIndex);
            Verify.AreEqual(expectedLastItemGroupIndex, actualLastItemGroupIndex);
            Verify.AreEqual(expectedLastItemIndex, actualLastItemIndex);
        }

        private void RunElementTrackingTestRoutine(Action<
            ObservableCollection<ObservableCollection<string>> /* data */,
            TestScrollingSurface[] /* scrollers */,
            ItemsRepeater /* rootRepeater */> testRoutine)
        {
            // Base setup for our element tracking tests.
            // We have 4 fake scrollers in series, initially in a non scrollable configuration.
            // Under them we have a group repeater tree (2 groups with 3 items each).
            // The group UI is a StackPanel with a TextBlock "header" and an inner ItemsRepeater.
            RunOnUIThread.Execute(() =>
            {
                int groupCount = 2;
                int itemsPerGroup = 3;

                var data = new ObservableCollection<ObservableCollection<string>>(
                    Enumerable.Range(0, groupCount).Select(i => new ObservableCollection<string>(
                        Enumerable.Range(0, itemsPerGroup).Select(j => string.Format("Item #{0}.{1}", i, j)))));

                var itemElements = Enumerable.Range(0, groupCount).Select(i =>
                    Enumerable.Range(0, itemsPerGroup).Select(j => new Border { Name = data[i][j], Width = 50, Height = 50, Background = new SolidColorBrush(Colors.Red) }).ToList()).ToArray();

                var headerElements = Enumerable.Range(0, groupCount).Select(i => new TextBlock { Text = "Header #" + i }).ToList();
                var groupRepeaters = Enumerable.Range(0, groupCount).Select(i => new ItemsRepeater
                {
                    Name = "ItemsRepeater #" + i,
                    ItemsSource = data[i],
                    ItemTemplate = (Microsoft.UI.Xaml.Controls.IElementFactoryShim)MockElementFactory.CreateElementFactory(itemElements[i]),
                    Layout = new TestGridLayout { Orientation = Orientation.Horizontal, MinItemWidth = 50, MinItemHeight = 50, MinRowSpacing = 10, MinColumnSpacing = 10 }
                }).ToList();
                var groupElement = Enumerable.Range(0, groupCount).Select(i =>
                {
                    var panel = new StackPanel();
                    panel.Name = "Group #" + i;
                    panel.Children.Add(headerElements[i]);
                    panel.Children.Add(groupRepeaters[i]);
                    return panel;
                }).ToList();

                var rootRepeater = new ItemsRepeater
                {
                    Name = "Root ItemsRepeater",
                    ItemsSource = data,
                    ItemTemplate = (Microsoft.UI.Xaml.Controls.IElementFactoryShim)MockElementFactory.CreateElementFactory(groupElement),
                    Layout = new TestStackLayout { Orientation = Orientation.Vertical }
                };

                var scrollers = new TestScrollingSurface[4];
                for (int i = 0; i < scrollers.Length; ++i)
                {
                    scrollers[i] = new TestScrollingSurface()
                    {
                        Name = "S" + i,
                        Content = i > 0 ? (UIElement)scrollers[i - 1] : rootRepeater
                    };
                }

                var resetScrollers = (Action)(() =>
                {
                    foreach (var scroller in scrollers)
                    {
                        scroller.IsHorizontallyScrollable = false;
                        scroller.IsVerticallyScrollable = false;
                        scroller.RegisterAnchorCandidateFunc = null;
                        scroller.UnregisterAnchorCandidateFunc = null;
                        scroller.GetRelativeViewportFunc = null;
                    }
                });

                var outerScroller = scrollers.Last();
                outerScroller.Width = 200.0;
                outerScroller.Height = 2000.0;

                Content = outerScroller;
                Content.UpdateLayout();

                testRoutine(data, scrollers, rootRepeater);
            });
        }

        private static VirtualizingLayout GetMonitoringLayout(Size desiredSize, List<Rect> realizationRects)
        {
            return new MockVirtualizingLayout
            {
                MeasureLayoutFunc = (availableSize, context) =>
                {
                    realizationRects.Add(context.RealizationRect);
                    return desiredSize;
                },

                ArrangeLayoutFunc = (finalSize, context) => finalSize
            };
        }

        private class TestScrollingSurface : ContentControl, IRepeaterScrollingSurface
        {
            private bool _isHorizontallyScrollable;
            private bool _isVerticallyScrollable;
            private EventRegistrationTokenTable<ConfigurationChangedEventHandler> _configurationChangedTokenTable;

            public bool InMeasure { get; set; }
            public bool InArrange { get; set; }
            public bool InPostArrange { get; private set; }

            public Action ConfigurationChangedAddFunc { get; set; }
            public Action ConfigurationChangedRemoveFunc { get; set; }

            public Action<UIElement> RegisterAnchorCandidateFunc { get; set; }
            public Action<UIElement> UnregisterAnchorCandidateFunc { get; set; }
            public Func<UIElement, Rect> GetRelativeViewportFunc { get; set; }

            public UIElement AnchorElement { get; set; }

            public bool IsHorizontallyScrollable
            {
                get { return _isHorizontallyScrollable; }
                set
                {
                    _isHorizontallyScrollable = value;
                    RaiseConfigurationChanged();
                    InvalidateMeasure();
                }
            }

            public bool IsVerticallyScrollable
            {
                get { return _isVerticallyScrollable; }
                set
                {
                    _isVerticallyScrollable = value;
                    RaiseConfigurationChanged();
                    InvalidateMeasure();
                }
            }

            public event ConfigurationChangedEventHandler ConfigurationChanged
            {
                add
                {
                    if (ConfigurationChangedAddFunc != null)
                    {
                        ConfigurationChangedAddFunc();
                    }

                    return EventRegistrationTokenTable<ConfigurationChangedEventHandler>
                        .GetOrCreateEventRegistrationTokenTable(ref _configurationChangedTokenTable)
                        .AddEventHandler(value);
                }
                remove
                {
                    if (ConfigurationChangedRemoveFunc != null)
                    {
                        ConfigurationChangedRemoveFunc();
                    }

                    EventRegistrationTokenTable<ConfigurationChangedEventHandler>
                        .GetOrCreateEventRegistrationTokenTable(ref _configurationChangedTokenTable)
                        .RemoveEventHandler(value);
                }
            }
            public event PostArrangeEventHandler PostArrange;
#pragma warning disable CS0067
            // Warning CS0067: The event 'ViewportTests.TestScrollingSurface.ViewportChanged' is never used.
            public event ViewportChangedEventHandler ViewportChanged;
#pragma warning restore CS0067

            public void RegisterAnchorCandidate(UIElement element)
            {
                RegisterAnchorCandidateFunc(element);
            }

            public void UnregisterAnchorCandidate(UIElement element)
            {
                UnregisterAnchorCandidateFunc(element);
            }

            public Rect GetRelativeViewport(UIElement child)
            {
                return GetRelativeViewportFunc(child);
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                InMeasure = true;
                var result = base.MeasureOverride(availableSize);
                InMeasure = false;
                return result;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                InArrange = true;

                var result = base.ArrangeOverride(finalSize);

                InArrange = false;
                InPostArrange = true;

                if (PostArrange != null)
                {
                    PostArrange(this);
                }

                InPostArrange = false;

                return result;
            }

            private void RaiseConfigurationChanged()
            {
                var temp =
                    EventRegistrationTokenTable<ConfigurationChangedEventHandler>
                    .GetOrCreateEventRegistrationTokenTable(ref _configurationChangedTokenTable)
                    .InvocationList;
                if (temp != null)
                {
                    temp(this);
                }
            }
        }

        private class TestStackLayout : StackLayout
        {
            public UIElement SuggestedAnchor { get; private set; }

            protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
            {
                var anchorIndex = context.RecommendedAnchorIndex;
                SuggestedAnchor = anchorIndex < 0 ? null : context.GetOrCreateElementAt(anchorIndex);
                return base.MeasureOverride(context, availableSize);
            }
        }

        private class TestGridLayout : UniformGridLayout
        {
            public UIElement SuggestedAnchor { get; private set; }

            protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
            {
                var anchorIndex = context.RecommendedAnchorIndex;
                SuggestedAnchor = anchorIndex < 0 ? null : context.GetOrCreateElementAt(anchorIndex);
                return base.MeasureOverride(context, availableSize);
            }
        }
    }
}

#endif