import React from 'react';

interface AnalyzerRule {
  id: string;
  category: string;
  severity: string;
  notes: string;
}

const rules: AnalyzerRule[] = require('../data/generated/analyzer-rules.json');

export default function AnalyzerRules(): JSX.Element {
  return (
    <table>
      <thead>
        <tr>
          <td>Rule ID</td>
          <td>Severity</td>
          <td>Description</td>
        </tr>
      </thead>
      <tbody>
        {rules.map((r) => (
          <tr>
            <td>{r.id}</td>
            <td>{r.severity}</td>
            <td>{r.notes}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
