﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#pragma once

#include "ViewChangeBase.h"
#include "ScrollerTrace.h"

enum class InteractionTrackerAsyncOperationType
{
    None,
    TryUpdatePosition,
    TryUpdatePositionBy,
    TryUpdatePositionWithAnimation,
    TryUpdatePositionWithAdditionalVelocity,
    TryUpdateScale,
    TryUpdateScaleWithAnimation,
    TryUpdateScaleWithAdditionalVelocity,
};

enum class InteractionTrackerAsyncOperationTrigger
{
    // Operation is triggered by a direct call to Scroller's ScrollTo/ScrollBy/ScrollFrom or ZoomTo/ZoomBy/ZoomFrom
    DirectViewChange = 0x01,
    // Operation is triggered by the horizontal IScrollController.
    HorizontalScrollControllerRequest = 0x02,
    // Operation is triggered by the vertical IScrollController.
    VerticalScrollControllerRequest = 0x04,
    // Operation is triggered by the mouse wheel.
    MouseWheel = 0x08,
};

// Used as a workaround for InteractionTracker bug "12465209 - InteractionTracker remains silent when calling TryUpdatePosition with the current position":
// Maximum number of UI thread ticks processed while waiting for non-animated operations to complete.
const int c_maxNonAnimatedOperationTicks = 10;

// Number of UI thread ticks elapsed before a queued operation gets processed to allow any pending size
// changes to be propagated to the InteractionTracker.
const int c_queuedOperationTicks = 3;

class InteractionTrackerAsyncOperation
{
public:
    InteractionTrackerAsyncOperation(
        InteractionTrackerAsyncOperationType operationType,
        InteractionTrackerAsyncOperationTrigger operationTrigger,
        bool isDelayed,
        std::shared_ptr<ViewChangeBase> viewChangeBase);
    ~InteractionTrackerAsyncOperation();

    int32_t GetViewChangeId() const
    {
        return m_viewChangeId;
    }

    void SetViewChangeId(int32_t viewChangeId)
    {
        m_viewChangeId = viewChangeId;
    }

    bool IsAnimated() const
    {
        switch (m_operationType)
        {
            case InteractionTrackerAsyncOperationType::TryUpdatePosition:
            case InteractionTrackerAsyncOperationType::TryUpdatePositionBy:
            case InteractionTrackerAsyncOperationType::TryUpdateScale:
                return false;
        }
        return true;
    }

    bool IsCanceled() const
    {
        return m_isCanceled;
    }

    void SetIsCanceled(bool isCanceled)
    {
        SCROLLER_TRACE_VERBOSE(nullptr, TRACE_MSG_METH_INT, METH_NAME, this, isCanceled);

        m_isCanceled = isCanceled;
    }

    bool IsDelayed() const
    {
        return m_isDelayed;
    }

    void SetIsDelayed(bool isDelayed)
    {
        SCROLLER_TRACE_VERBOSE(nullptr, TRACE_MSG_METH_INT, METH_NAME, this, isDelayed);

        m_isDelayed = isDelayed;
    }

    bool IsQueued() const
    {
        return m_preProcessingTicksCountdown > 0;
    }

    bool IsUnqueueing() const
    {
        return m_preProcessingTicksCountdown > 0 && m_preProcessingTicksCountdown < m_queuedOperationTicks;
    }

    int GetTicksCountdown() const
    {
        return m_preProcessingTicksCountdown;
    }

    void SetTicksCountdown(int ticksCountdown)
    {
        SCROLLER_TRACE_VERBOSE(nullptr, TRACE_MSG_METH_INT, METH_NAME, this, ticksCountdown);

        MUX_ASSERT(ticksCountdown > 0);
        m_preProcessingTicksCountdown = m_queuedOperationTicks = ticksCountdown;
    }

    InteractionTrackerAsyncOperationTrigger GetOperationTrigger() const
    {
        return m_operationTrigger;
    }

    // Returns True when the operation fulfills a horizontal IScrollController request.
    bool IsHorizontalScrollControllerRequest() const
    {
        return static_cast<int>(m_operationTrigger) & static_cast<int>(InteractionTrackerAsyncOperationTrigger::HorizontalScrollControllerRequest);
    }

    // Returns True when the operation fulfills a vertical IScrollController request.
    bool IsVerticalScrollControllerRequest() const
    {
        return static_cast<int>(m_operationTrigger) & static_cast<int>(InteractionTrackerAsyncOperationTrigger::VerticalScrollControllerRequest);
    }

    void SetIsScrollControllerRequest(bool isFromHorizontalScrollController)
    {
        SCROLLER_TRACE_VERBOSE(nullptr, TRACE_MSG_METH_INT, METH_NAME, this, isFromHorizontalScrollController);

        if (isFromHorizontalScrollController)
            m_operationTrigger = static_cast<InteractionTrackerAsyncOperationTrigger>(static_cast<int>(m_operationTrigger) | 
                                 static_cast<int>(InteractionTrackerAsyncOperationTrigger::HorizontalScrollControllerRequest));
        else
            m_operationTrigger = static_cast<InteractionTrackerAsyncOperationTrigger>(static_cast<int>(m_operationTrigger) |
                                 static_cast<int>(InteractionTrackerAsyncOperationTrigger::VerticalScrollControllerRequest));
    }

    void TickNonAnimatedOperation(bool* needsCompletion)
    {
        MUX_ASSERT(!IsAnimated());
        MUX_ASSERT(m_postProcessingTicksCountdown > 0);

        m_postProcessingTicksCountdown--;
        *needsCompletion = m_postProcessingTicksCountdown == 0;

        SCROLLER_TRACE_VERBOSE(nullptr, TRACE_MSG_METH_INT, METH_NAME, this, m_postProcessingTicksCountdown);
    }

    void TickQueuedOperation(bool* needsProcessing)
    {
        MUX_ASSERT(m_preProcessingTicksCountdown > 0);

        m_preProcessingTicksCountdown--;
        *needsProcessing = m_preProcessingTicksCountdown == 0;

        SCROLLER_TRACE_VERBOSE(nullptr, TRACE_MSG_METH_INT, METH_NAME, this, m_preProcessingTicksCountdown);
    }

    InteractionTrackerAsyncOperationType GetOperationType() const
    {
        return m_operationType;
    }

    int GetRequestId() const
    {
        return m_requestId;
    }

    void SetRequestId(int requestId)
    {
        SCROLLER_TRACE_VERBOSE(nullptr, TRACE_MSG_METH_INT, METH_NAME, this, requestId);

        m_requestId = requestId;
    }

    std::shared_ptr<ViewChangeBase> GetViewChangeBase() const
    {
        return m_viewChangeBase;
    }

private:
    // Identifies the InteractionTracker request type for this operation.
    InteractionTrackerAsyncOperationType m_operationType{ InteractionTrackerAsyncOperationType::None };

    // Identifies the InteractionTracker trigger type for this operation.
    InteractionTrackerAsyncOperationTrigger m_operationTrigger{ InteractionTrackerAsyncOperationTrigger::DirectViewChange };

    // Number of UI thread ticks remaining before a non-animated InteractionTracker request is declared completed
    // in case no ValuesChanged or status change notification is raised.
    int m_postProcessingTicksCountdown{ 0 };

    // Number of UI thread ticks remaining before this queued operation gets processed.
    // Positive between the time the operation is queued in Scroller::ScrollTo/By/From, Scroller::ZoomTo/By/From or
    // Scroller::OnCompositionTargetRendering and the time it is processed in Scroller::ProcessOffsetsChange or Scroller::ProcessZoomFactorChange.
    int m_preProcessingTicksCountdown{ c_queuedOperationTicks };

    // Initial value of m_preProcessingTicksCountdown when this operation is queued up.
    int m_queuedOperationTicks{ c_queuedOperationTicks };

    // InteractionTracker RequestId associated with this operation.
    int m_requestId{ -1 };

    // Set to True when the operation was canceled early enough to take effect.
    bool m_isCanceled{ false };

    // Set to True when the operation is delayed until the scroller is loaded.
    bool m_isDelayed{ false };

    // OffsetsChange or ZoomFactorChange instance associated with this operation.
    std::shared_ptr<ViewChangeBase> m_viewChangeBase;

    // ViewChangeId associated with this operation.
    int32_t m_viewChangeId{ -1 };
};

