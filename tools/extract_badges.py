import sys, re
sys.stdout.reconfigure(encoding='utf-8', errors='replace')

with open(r'C:/Users/ryu/Downloads/mcv/crates/plugin-kick/src/3782-c250e2d38878d546.js', 'r', encoding='utf-8') as f:
    content = f.read()

# React prop name -> SVG attribute name mapping
PROP_TO_SVG = {
    'viewBox': 'viewBox',
    'xmlns': 'xmlns',
    'width': 'width',
    'height': 'height',
    'fill': 'fill',
    'fillRule': 'fill-rule',
    'clipRule': 'clip-rule',
    'clipPath': 'clip-path',
    'stopColor': 'stop-color',
    'stopOpacity': 'stop-opacity',
    'gradientUnits': 'gradientUnits',
    'gradientTransform': 'gradientTransform',
    'x1': 'x1', 'y1': 'y1', 'x2': 'x2', 'y2': 'y2',
    'cx': 'cx', 'cy': 'cy', 'r': 'r',
    'fx': 'fx', 'fy': 'fy',
    'offset': 'offset',
    'rx': 'rx', 'ry': 'ry',
    'd': 'd',
    'id': 'id',
    'stroke': 'stroke',
    'strokeWidth': 'stroke-width',
    'strokeLinecap': 'stroke-linecap',
    'strokeLinejoin': 'stroke-linejoin',
    'opacity': 'opacity',
    'transform': 'transform',
    'x': 'x', 'y': 'y',
}

SELF_CLOSING = {'path', 'stop', 'rect', 'circle', 'ellipse', 'line', 'polyline', 'polygon', 'use', 'image'}


def parse_jsx_call(text, pos):
    m = re.match(r'\(0,\s*s\.jsxs?\)', text[pos:])
    if not m:
        return None
    pos += m.end()

    if pos >= len(text) or text[pos] != '(':
        return None
    pos += 1

    m = re.match(r'"([^"]+)"', text[pos:])
    if not m:
        return None
    element_name = m.group(1)
    pos += m.end()

    while pos < len(text) and text[pos] in ' ,\t\n\r':
        pos += 1

    if pos >= len(text) or text[pos] != '{':
        return None

    props, children, pos = parse_props_object(text, pos)

    while pos < len(text) and text[pos] in ' \t\n\r':
        pos += 1
    if pos < len(text) and text[pos] == ')':
        pos += 1

    return element_name, props, children, pos


def parse_props_object(text, pos):
    assert text[pos] == '{', f"Expected '{{' at {pos}"
    pos += 1

    props = {}
    children = []

    while pos < len(text):
        while pos < len(text) and text[pos] in ' \t\n\r':
            pos += 1

        if pos >= len(text) or text[pos] == '}':
            pos += 1
            break

        if text[pos] == ',':
            pos += 1
            continue

        # Spread operator
        m = re.match(r'\.\.\.\w+', text[pos:])
        if m:
            pos += m.end()
            continue

        m = re.match(r'(\w+)', text[pos:])
        if not m:
            pos += 1
            continue

        prop_name = m.group(1)
        pos += m.end()

        while pos < len(text) and text[pos] in ' \t\n\r':
            pos += 1

        if pos >= len(text) or text[pos] != ':':
            continue
        pos += 1

        while pos < len(text) and text[pos] in ' \t\n\r':
            pos += 1

        if prop_name == 'children':
            if text[pos] == '[':
                children, pos = parse_children_array(text, pos)
            elif text[pos:pos+3] == '(0,':
                result = parse_jsx_call(text, pos)
                if result:
                    el, p, ch, pos = result
                    children = [build_svg_element(el, p, ch)]
                else:
                    pos += 1
            elif pos < len(text) and text[pos] == '"':
                m = re.match(r'"([^"]*)"', text[pos:])
                if m:
                    children = [m.group(1)]
                    pos += m.end()
            else:
                pos += 1
        elif pos < len(text) and text[pos] == '"':
            m = re.match(r'"([^"]*)"', text[pos:])
            if m:
                props[prop_name] = m.group(1)
                pos += m.end()
        elif text[pos:pos+3] == '(0,':
            # Function call value - skip balanced parens
            depth = 0
            while pos < len(text):
                if text[pos] == '(':
                    depth += 1
                elif text[pos] == ')':
                    depth -= 1
                    if depth == 0:
                        pos += 1
                        break
                pos += 1
        elif text[pos:pos+4] == 'null':
            pos += 4
        elif text[pos:pos+4] == 'void':
            pos += 4
            while pos < len(text) and text[pos] not in ',\n}':
                pos += 1
        else:
            m = re.match(r'[\w.+\-]+', text[pos:])
            if m:
                props[prop_name] = m.group(0)
                pos += m.end()
            else:
                pos += 1

    return props, children, pos


def parse_children_array(text, pos):
    assert text[pos] == '['
    pos += 1
    children = []

    while pos < len(text):
        while pos < len(text) and text[pos] in ' \t\n\r,':
            pos += 1

        if pos >= len(text) or text[pos] == ']':
            pos += 1
            break

        if text[pos:pos+3] == '(0,':
            result = parse_jsx_call(text, pos)
            if result:
                el, p, ch, pos = result
                children.append(build_svg_element(el, p, ch))
            else:
                pos += 1
        else:
            pos += 1

    return children, pos


def build_svg_element(tag, props, children):
    attrs = []
    for react_prop, value in props.items():
        svg_attr = PROP_TO_SVG.get(react_prop, react_prop)
        attrs.append(f'{svg_attr}="{value}"')

    attr_str = (' ' + ' '.join(attrs)) if attrs else ''

    if tag in SELF_CLOSING and not children:
        return f'<{tag}{attr_str}/>'
    else:
        children_str = ''.join(str(c) for c in children)
        return f'<{tag}{attr_str}>{children_str}</{tag}>'


badge_ranges = [
    ('broadcaster', 1202526, 1208164),
    ('moderator', 1208164, 1216510),
    ('og', 1216510, 1227413),
    ('sidekick', 1227413, 1231334),
    ('staff', 1231334, 1234236),
    ('sub_gifter', 1234236, 1236535),
    ('subscriber', 1236535, 1240473),
    ('vip', 1240473, 1245000),
]

svgs = {}

for badge, start, end in badge_ranges:
    chunk = content[start:min(end, start + 15000)]
    # Find the JSX call starting position
    jsx_start = -1
    for pattern in ['(0,\n                    s.jsxs)', '(0,\n                s.jsxs)', '(0, s.jsxs)']:
        idx = chunk.find(pattern)
        if idx >= 0:
            jsx_start = idx
            break

    if jsx_start < 0:
        print(f"{badge}: JSX not found")
        continue

    try:
        result = parse_jsx_call(chunk, jsx_start)
        if result:
            el, props, children, end_pos = result
            svg_str = build_svg_element(el, props, children)
            svgs[badge] = svg_str
            print(f"{badge}: OK (length={len(svg_str)})")
        else:
            print(f"{badge}: parse failed")
    except Exception as e:
        import traceback
        print(f"{badge}: ERROR - {e}")
        traceback.print_exc()

# Save SVGs to files
import os, urllib.parse, base64
out_dir = r'C:/Users/ryu/Downloads/mcv/crates/plugin-kick/src/badges'
os.makedirs(out_dir, exist_ok=True)

for badge, svg in svgs.items():
    svg_path = os.path.join(out_dir, f'{badge}.svg')
    with open(svg_path, 'w', encoding='utf-8') as f:
        f.write(svg)

    # Also create data URI
    encoded = urllib.parse.quote(svg, safe='')
    data_uri = f'data:image/svg+xml,{encoded}'
    print(f"  {badge} data URI length: {len(data_uri)}")

print("\nSVG files saved to:", out_dir)
