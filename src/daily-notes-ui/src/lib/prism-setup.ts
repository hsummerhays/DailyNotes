// Import init FIRST to ensure window.Prism is set
import './prism-init';

// Now import components that depend on window.Prism
import 'prismjs/components/prism-clike';
import 'prismjs/components/prism-javascript';
import 'prismjs/components/prism-typescript';
import 'prismjs/components/prism-bash';
import 'prismjs/components/prism-markdown';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-python';
import 'prismjs/components/prism-json';

import Prism from 'prismjs';
export default Prism;
