import sys, json, re
sys.stdout = open(sys.stdout.fileno(), mode='w', encoding='utf-8', buffering=1)

with open('c:/Users/ryu/Downloads/mcv/crates/plugin-youtube-live/src/streams_test.html', 'r', encoding='utf-8', errors='replace') as f:
    body = f.read()

print('File size:', len(body))
print('Has Gr_pTQgzHow:', 'Gr_pTQgzHow' in body)
print('Has iLNXliI0dAI:', 'iLNXliI0dAI' in body)
print()

# Parse ytInitialData
prefix = 'var ytInitialData = '
idx = body.find(prefix)
if idx < 0:
    prefix = 'window["ytInitialData"] = '
    idx = body.find(prefix)
if idx < 0:
    print('ytInitialData NOT FOUND')
    sys.exit(1)

json_start = idx + len(prefix)
depth = 0; in_str = False; esc = False; end = json_start
for i, c in enumerate(body[json_start:]):
    if esc: esc = False; continue
    if in_str:
        if c == chr(92): esc = True
        elif c == '"': in_str = False
        continue
    if c == '"': in_str = True
    elif c == '{': depth += 1
    elif c == '}':
        depth -= 1
        if depth == 0: end = json_start + i + 1; break

data = json.loads(body[json_start:end])

# Find videoRenderer items with isLive or LIVE badge
def find_video_renderers(obj, results=None):
    if results is None:
        results = []
    if isinstance(obj, dict):
        # Check if this is a videoRenderer
        if 'videoId' in obj and ('title' in obj or 'thumbnailOverlays' in obj):
            vid = obj.get('videoId')
            # Get title
            title_obj = obj.get('title', {})
            title = ''
            if 'runs' in title_obj:
                title = ''.join(r.get('text', '') for r in title_obj['runs'])
            elif 'simpleText' in title_obj:
                title = title_obj['simpleText']
            # Check for LIVE badge
            overlays = obj.get('thumbnailOverlays', [])
            is_live = False
            for overlay in overlays:
                status = overlay.get('thumbnailOverlayTimeStatusRenderer', {})
                if status.get('style') == 'LIVE':
                    is_live = True
                    break
            # Also check badges
            badges = obj.get('badges', [])
            for badge in badges:
                meta = badge.get('metadataBadgeRenderer', {})
                if meta.get('style') == 'BADGE_STYLE_TYPE_LIVE_NOW':
                    is_live = True
                    break
            if vid:
                results.append({'videoId': vid, 'title': title, 'isLive': is_live})
        for v in obj.values():
            find_video_renderers(v, results)
    elif isinstance(obj, list):
        for v in obj:
            find_video_renderers(v, results)
    return results

all_videos = find_video_renderers(data)
# Deduplicate by videoId
seen = {}
for v in all_videos:
    vid = v['videoId']
    if vid not in seen:
        seen[vid] = v

print(f'Total unique videos found: {len(seen)}')
print()
print('=== LIVE videos ===')
for vid, v in seen.items():
    if v['isLive']:
        print(f"  videoId={vid}")
        print(f"  title={v['title'][:80]}")
        print()

print('=== First 5 non-live videos ===')
count = 0
for vid, v in seen.items():
    if not v['isLive']:
        print(f"  videoId={vid}, title={v['title'][:60]}")
        count += 1
        if count >= 5:
            break
