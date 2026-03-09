import { useEffect, useState, useCallback } from 'react';
import { LexicalComposer } from '@lexical/react/LexicalComposer';
import { RichTextPlugin } from '@lexical/react/LexicalRichTextPlugin';
import { ContentEditable } from '@lexical/react/LexicalContentEditable';
import { HistoryPlugin } from '@lexical/react/LexicalHistoryPlugin';
import { OnChangePlugin } from '@lexical/react/LexicalOnChangePlugin';
import { ListPlugin } from '@lexical/react/LexicalListPlugin';
import { LinkPlugin } from '@lexical/react/LexicalLinkPlugin';
import { LexicalErrorBoundary } from '@lexical/react/LexicalErrorBoundary';
import { useLexicalComposerContext } from '@lexical/react/LexicalComposerContext';
import {
    FORMAT_TEXT_COMMAND,
    SELECTION_CHANGE_COMMAND,
    CAN_REDO_COMMAND,
    CAN_UNDO_COMMAND,
    REDO_COMMAND,
    UNDO_COMMAND,
    $getSelection,
    $isRangeSelection,
    $createParagraphNode,
    $createTextNode,
    $getRoot,
} from 'lexical';
import {
    INSERT_ORDERED_LIST_COMMAND,
    INSERT_UNORDERED_LIST_COMMAND,
} from '@lexical/list';
import {
    TOGGLE_LINK_COMMAND,
    $isLinkNode,
} from '@lexical/link';
import {
    HeadingNode, QuoteNode,
} from '@lexical/rich-text';
import { ListNode, ListItemNode } from '@lexical/list';
import { LinkNode } from '@lexical/link';
import {
    CodeNode,
    CodeHighlightNode,
    $createCodeNode,
    $isCodeNode,
    registerCodeHighlighting
} from '@lexical/code';
import { mergeRegister } from '@lexical/utils';
import {
    Bold, Italic, Underline as UnderlineIcon, Link as LinkIcon,
    List, ListOrdered, Undo, Redo, Code
} from 'lucide-react';

import '../lib/prism-setup';

const theme = {
    ltr: 'ltr',
    rtl: 'rtl',
    placeholder: 'editor-placeholder',
    paragraph: 'editor-paragraph',
    quote: 'editor-quote',
    heading: {
        h1: 'editor-heading-h1',
        h2: 'editor-heading-h2',
        h3: 'editor-heading-h3',
    },
    list: {
        nested: {
            listitem: 'editor-nested-listitem',
        },
        ol: 'editor-list-ol',
        ul: 'editor-list-ul',
        listitem: 'editor-listitem',
    },
    code: 'editor-code',
    codeHighlight: {
        atrule: 'editor-tokenAtrule',
        attr: 'editor-tokenAttr',
        boolean: 'editor-tokenBoolean',
        builtin: 'editor-tokenBuiltin',
        cdata: 'editor-tokenCdata',
        char: 'editor-tokenChar',
        class: 'editor-tokenClass',
        'class-name': 'editor-tokenClassName',
        comment: 'editor-tokenComment',
        constant: 'editor-tokenConstant',
        deleted: 'editor-tokenDeleted',
        doctype: 'editor-tokenDoctype',
        entity: 'editor-tokenEntity',
        function: 'editor-tokenFunction',
        important: 'editor-tokenImportant',
        inserted: 'editor-tokenInserted',
        keyword: 'editor-tokenKeyword',
        namespace: 'editor-tokenNamespace',
        number: 'editor-tokenNumber',
        operator: 'editor-tokenOperator',
        prolog: 'editor-tokenProlog',
        property: 'editor-tokenProperty',
        punctuation: 'editor-tokenPunctuation',
        regex: 'editor-tokenRegex',
        selector: 'editor-tokenSelector',
        string: 'editor-tokenString',
        symbol: 'editor-tokenSymbol',
        tag: 'editor-tokenTag',
        url: 'editor-tokenUrl',
        variable: 'editor-tokenVariable',
    },
    text: {
        bold: 'editor-text-bold',
        italic: 'editor-text-italic',
        underline: 'editor-text-underline',
        strikethrough: 'editor-text-strikethrough',
        underlineStrikethrough: 'editor-text-underlineStrikethrough',
    },
    link: 'editor-link',
};

function ToolbarPlugin() {
    const [editor] = useLexicalComposerContext();
    const [isBold, setIsBold] = useState(false);
    const [isItalic, setIsItalic] = useState(false);
    const [isUnderline, setIsUnderline] = useState(false);
    const [isLink, setIsLink] = useState(false);
    const [isCode, setIsCode] = useState(false);
    const [canUndo, setCanUndo] = useState(false);
    const [canRedo, setCanRedo] = useState(false);

    const updateToolbar = useCallback(() => {
        const selection = $getSelection();
        if ($isRangeSelection(selection)) {
            setIsBold(selection.hasFormat('bold'));
            setIsItalic(selection.hasFormat('italic'));
            setIsUnderline(selection.hasFormat('underline'));

            const node = selection.getNodes()[0];
            const parent = node?.getParent();
            setIsLink($isLinkNode(parent) || $isLinkNode(node));
            setIsCode($isCodeNode(parent) || $isCodeNode(node));
        }
    }, []);

    useEffect(() => {
        return mergeRegister(
            editor.registerUpdateListener(({ editorState }) => {
                editorState.read(() => {
                    updateToolbar();
                });
            }),
            editor.registerCommand(
                SELECTION_CHANGE_COMMAND,
                () => {
                    updateToolbar();
                    return false;
                },
                1
            ),
            editor.registerCommand(
                CAN_UNDO_COMMAND,
                (payload) => {
                    setCanUndo(payload);
                    return false;
                },
                1
            ),
            editor.registerCommand(
                CAN_REDO_COMMAND,
                (payload) => {
                    setCanRedo(payload);
                    return false;
                },
                1
            )
        );
    }, [editor, updateToolbar]);

    const insertLink = useCallback(() => {
        if (!isLink) {
            const url = window.prompt('Enter the URL');
            if (url) {
                editor.dispatchCommand(TOGGLE_LINK_COMMAND, url);
            }
        } else {
            editor.dispatchCommand(TOGGLE_LINK_COMMAND, null);
        }
    }, [editor, isLink]);

    const toggleCode = useCallback(() => {
        editor.update(() => {
            const selection = $getSelection();
            if ($isRangeSelection(selection)) {
                if (isCode) {
                    const codeNode = selection.getNodes()[0].getParent();
                    if ($isCodeNode(codeNode)) {
                        codeNode.replace($createParagraphNode());
                    }
                } else {
                    const codeNode = $createCodeNode();
                    selection.insertNodes([codeNode]);
                }
            }
        });
    }, [editor, isCode]);

    return (
        <div className="editor-toolbar">
            <button
                type="button"
                onClick={() => editor.dispatchCommand(FORMAT_TEXT_COMMAND, 'bold')}
                className={`toolbar-btn ${isBold ? 'is-active' : ''}`}
                title="Bold"
            >
                <Bold size={16} />
            </button>
            <button
                type="button"
                onClick={() => editor.dispatchCommand(FORMAT_TEXT_COMMAND, 'italic')}
                className={`toolbar-btn ${isItalic ? 'is-active' : ''}`}
                title="Italic"
            >
                <Italic size={16} />
            </button>
            <button
                type="button"
                onClick={() => editor.dispatchCommand(FORMAT_TEXT_COMMAND, 'underline')}
                className={`toolbar-btn ${isUnderline ? 'is-active' : ''}`}
                title="Underline"
            >
                <UnderlineIcon size={16} />
            </button>
            <button
                type="button"
                onClick={insertLink}
                className={`toolbar-btn ${isLink ? 'is-active' : ''}`}
                title="Link"
            >
                <LinkIcon size={16} />
            </button>
            <button
                type="button"
                onClick={toggleCode}
                className={`toolbar-btn ${isCode ? 'is-active' : ''}`}
                title="Code Block"
            >
                <Code size={16} />
            </button>

            <div style={{ width: '1px', background: 'var(--color-border)', margin: '0 0.25rem' }} />

            <button
                type="button"
                onClick={() => editor.dispatchCommand(INSERT_UNORDERED_LIST_COMMAND, undefined)}
                className="toolbar-btn"
                title="Bullet List"
            >
                <List size={16} />
            </button>
            <button
                type="button"
                onClick={() => editor.dispatchCommand(INSERT_ORDERED_LIST_COMMAND, undefined)}
                className="toolbar-btn"
                title="Numbered List"
            >
                <ListOrdered size={16} />
            </button>

            <div style={{ marginLeft: 'auto', display: 'flex', gap: '0.25rem' }}>
                <button
                    type="button"
                    onClick={() => editor.dispatchCommand(UNDO_COMMAND, undefined)}
                    className="toolbar-btn"
                    disabled={!canUndo}
                    title="Undo"
                >
                    <Undo size={16} />
                </button>
                <button
                    type="button"
                    onClick={() => editor.dispatchCommand(REDO_COMMAND, undefined)}
                    className="toolbar-btn"
                    disabled={!canRedo}
                    title="Redo"
                >
                    <Redo size={16} />
                </button>
            </div>
        </div>
    );
}

interface RichTextEditorProps {
    value: any;
    onChange: (value: any) => void;
    placeholder?: string;
}

export default function RichTextEditor({ value, onChange, placeholder = 'Write something...' }: RichTextEditorProps) {
    const initialConfig = {
        namespace: 'DailyNotesEditor',
        theme,
        onError: (error: Error) => {
            console.error(error);
        },
        nodes: [
            HeadingNode,
            QuoteNode,
            ListNode,
            ListItemNode,
            LinkNode,
            CodeNode,
            CodeHighlightNode
        ],
    };

    return (
        <LexicalComposer initialConfig={{
            ...initialConfig,
            editorState: typeof value === 'object' && value?.root ? JSON.stringify(value) : undefined
        }}>
            <div className="editor-container">
                <ToolbarPlugin />
                <div className="editor-inner" style={{ position: 'relative' }}>
                    <RichTextPlugin
                        contentEditable={<ContentEditable className="editor-input" />}
                        placeholder={<div className="editor-placeholder">{placeholder}</div>}
                        ErrorBoundary={LexicalErrorBoundary}
                    />
                    <HistoryPlugin />
                    <ListPlugin />
                    <LinkPlugin />
                    <LocalCodeHighlightPlugin />
                    <OnChangePlugin onChange={(editorState) => onChange(editorState.toJSON())} />
                    <LegacyDataHandler value={value} />
                </div>
            </div>
        </LexicalComposer>
    );
}

function LocalCodeHighlightPlugin() {
    const [editor] = useLexicalComposerContext();
    useEffect(() => {
        return registerCodeHighlighting(editor);
    }, [editor]);
    return null;
}

function LegacyDataHandler({ value }: { value: any }) {
    const [editor] = useLexicalComposerContext();
    const [lastExternalValue, setLastExternalValue] = useState<any>(null);

    useEffect(() => {
        if (value === lastExternalValue) return;
        setLastExternalValue(value);

        if (!value) return;

        editor.update(() => {
            if (typeof value === 'object' && value.root) {
                try {
                    const state = editor.parseEditorState(JSON.stringify(value));
                    editor.setEditorState(state);
                } catch (e) {
                    console.error('Failed to parse Lexical state', e);
                }
                return;
            }

            const root = $getRoot();
            root.clear();
            const p = $createParagraphNode();
            let text = '';
            if (typeof value === 'object') {
                text = typeof value.text === 'string' ? value.text : JSON.stringify(value);
            } else {
                text = String(value);
            }
            p.append($createTextNode(text));
            root.append(p);
        });
    }, [editor, value, lastExternalValue]);

    return null;
}
