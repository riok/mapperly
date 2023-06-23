import { visit } from 'unist-util-visit';
import { toHtml } from 'hast-util-to-html';

/*
  Adds FAQ structured data to the faq page.
  All pages ending with faq.md are considered.
  An FAQ page needs the following structure to be transformed correctly:
  H1 with an id 'faq',
  everything that follows this h1 is considered part of the faq until another h1 is encountered.
  Each h2 tag is then considered a question,
  everything that follows is considered the answer of the question,
  until a new h2 is encountered.

  If working on this script,
  testing changes needs the .docusaurus cache directory to be cleared.
  Eg. `npm run clear; npm run start`
*/

const faqFileName = 'faq.md';
const faqStartTagName = 'h1';
const faqStartId = 'faq';
const faqQuestionTagName = 'h2';

function addQuestion(faqData, questionData) {
  faqData.mainEntity.push({
    '@type': 'Question',
    name: toHtml(questionData.question),
    acceptedAnswer: {
      '@type': 'Answer',
      text: toHtml(questionData.answer),
    },
  });
}

function addFaqDataToTree(tree, faqData) {
  const faqScript = {
    type: 'element',
    tagName: 'script',
    properties: {
      type: 'application/ld+json',
    },
    children: [
      {
        type: 'text',
        value: JSON.stringify(faqData),
      },
    ],
  };
  tree.children = [faqScript, ...tree.children];
}

export default function rehypeFaq() {
  return (tree, file) => {
    if (!file.history[0].endsWith(faqFileName)) {
      return;
    }

    const faqData = {
      '@context': 'https://schema.org',
      '@type': 'FAQPage',
      mainEntity: [],
    };

    let inFaq = false;
    let currentEntry = undefined;
    visit(tree, 'element', (node) => {
      if (
        node.tagName === faqStartTagName &&
        node.properties.id === faqStartId
      ) {
        inFaq = true;
        return;
      }

      if (node.tagName === faqStartTagName) {
        inFaq = false;
        return;
      }

      if (!inFaq) {
        return;
      }

      if (node.tagName === faqQuestionTagName) {
        if (currentEntry !== undefined) {
          addQuestion(faqData, currentEntry);
        }

        currentEntry = {
          question: node.children,
          answer: [],
        };
        return;
      }

      if (currentEntry !== undefined) {
        currentEntry.answer.push(node);
      }
    });

    if (currentEntry === undefined) {
      return;
    }

    addQuestion(faqData, currentEntry);
    addFaqDataToTree(tree, faqData);
  };
}
