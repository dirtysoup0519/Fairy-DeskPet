using System;
using System.Collections.Generic;
using System.Linq;

namespace CodexFairy {
public sealed class GameObservation : IDisposable {
    readonly List<ScreenFrame> highlights = new List<ScreenFrame>();
    readonly Queue<KeyValuePair<DateTime, bool>> motion = new Queue<KeyValuePair<DateTime, bool>>();
    float[] previous, stable;
    DateTime stableSince, lastMotion, lastHighlight, detailSince, lastActivity;
    bool hasVisibleFrame;
    float[] detailCandidate;
    bool changing, motionNeedsReview;

    public int Event { get; private set; }
    public int Opportunity { get; private set; }
    public int Count { get { return highlights.Count; } }

    public bool IsBusy(DateTime now) {
        return changing || (now - lastMotion).TotalSeconds < 2;
    }

    public bool ShouldReviewActivePlay(DateTime now, DateTime lastRequest, int interval) {
        return hasVisibleFrame && lastActivity != DateTime.MinValue &&
            (now - lastActivity).TotalSeconds <= 3 &&
            (now - lastRequest).TotalSeconds >= Math.Max(8, interval);
    }

    public void Observe(ScreenFrame frame, int explorationInterval = 8) {
        var now = frame.At;
        double difference = previous == null ? 0 : ScreenFrames.Difference(previous, frame.Grid);
        previous = frame.Grid;
        motion.Enqueue(new KeyValuePair<DateTime, bool>(now, difference >= 8));
        while (motion.Count > 0 && (now - motion.Peek().Key).TotalSeconds > 4) motion.Dequeue();
        if (motion.Count >= 4 && motion.Count(sample => sample.Value) * 2 >= motion.Count) {
            lastMotion = now;
            motionNeedsReview = true;
        }

        double brightness = 0;
        for (int i = 2; i < frame.Grid.Length; i += 3) brightness += frame.Grid[i];
        hasVisibleFrame = brightness / (frame.Grid.Length / 3) >= 5;
        if (!hasVisibleFrame) {
            changing = true;
            stableSince = now;
            return;
        }

        if (difference >= 1 && motion.Count > 1) lastActivity = now;
        if (stable == null || ScreenFrames.Difference(stable, frame.Grid) >= 8) changing = true;
        if (difference >= 5 || stableSince == DateTime.MinValue) stableSince = now;
        bool settled = (now - stableSince).TotalSeconds >= 1 && (now - lastMotion).TotalSeconds >= 2;
        bool detailChanged = !changing && settled && highlights.Count > 0 &&
            ScreenFrames.LocalDifference(highlights.Last().Grid, frame.Grid) >= 8;
        if (!detailChanged) detailCandidate = null;
        else if (detailCandidate == null || ScreenFrames.LocalDifference(detailCandidate, frame.Grid) >= 5) {
            detailCandidate = frame.Grid;
            detailSince = now;
        }
        bool persistentDetail = detailCandidate != null && (now - detailSince).TotalMilliseconds >= 750;

        if (changing && settled) {
            stable = frame.Grid;
            changing = false;
            Event++;
            Opportunity++;
            motionNeedsReview = false;
            Keep(frame);
        } else if (!IsBusy(now) && settled && (motionNeedsReview ||
                   (now - lastHighlight).TotalSeconds >= explorationInterval && highlights.Count > 0 &&
                   (ScreenFrames.Difference(highlights.Last().Grid, frame.Grid) >= 3 || persistentDetail))) {
            Opportunity++;
            motionNeedsReview = false;
            Keep(frame);
        }
    }

    void Keep(ScreenFrame frame) {
        highlights.Add(new ScreenFrame(ScreenFrames.Resize(frame.Pixels, 640), frame.At, frame.Grid));
        lastHighlight = frame.At;
        detailCandidate = null;
        if (highlights.Count <= 8) return;
        highlights[0].Dispose();
        highlights.RemoveAt(0);
    }

    public ScreenAttachment Attach(ScreenFrame latest) {
        var recent = highlights.Where(frame => frame.At < latest.At &&
            (latest.At - frame.At).TotalSeconds <= 120 &&
            (ScreenFrames.Difference(frame.Grid, latest.Grid) >= 2 || ScreenFrames.LocalDifference(frame.Grid, latest.Grid) >= 8)).Reverse().Take(3).Reverse();
        var attachment = ScreenAttachment.Compose(recent.Concat(new[]{latest}).ToArray());
        attachment.Description += " 最后一帧是最新画面；前面的关键帧仅用于回顾，不能据此报告当前敌人位置或资源。";
        return attachment;
    }

    public void Dispose() {
        foreach (var frame in highlights) frame.Dispose();
        highlights.Clear();
        motion.Clear();
        previous = stable = detailCandidate = null;
        stableSince = lastMotion = lastHighlight = lastActivity = DateTime.MinValue;
        hasVisibleFrame = false;
        changing = motionNeedsReview = false;
        Event++;
        Opportunity++;
    }
}
}
