"""Makes the copy of the roadmap that is published as an artifact.

The artifact is one page with nothing beside it: it cannot read img/, and the
host supplies the doctype, the head and a small reset. So the wrapper comes off
and every picture goes in as a data URI.

    python development/build-artifact.py <output.html>
"""
import base64
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def build() -> str:
    page = io.open(os.path.join(HERE, 'roadmap.html'), encoding='utf-8').read()

    page = page[page.index('<title>'):]
    page = re.sub(r'<style>\s*html \{ color-scheme: light dark; \}.*?</style>\s*',
                  '', page, count=1, flags=re.S)
    page = page.replace('</head>\n<body>\n', '', 1)
    page = re.sub(r'\s*</body>\s*</html>\s*$', '\n', page)

    def inline(match):
        name = match.group(1)
        data = io.open(os.path.join(HERE, name.replace('/', os.sep)), 'rb').read()
        return 'src="data:image/png;base64,' + base64.b64encode(data).decode('ascii') + '"'

    return re.sub(r'src="(img/[^"]+)"', inline, page)


if __name__ == '__main__':
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, 'roadmap-artifact.html')
    text = build()
    io.open(out, 'w', encoding='utf-8').write(text)
    print('%s, %d KB, %d pictures' % (out, len(text.encode('utf-8')) // 1024,
                                      text.count('data:image/png')))
